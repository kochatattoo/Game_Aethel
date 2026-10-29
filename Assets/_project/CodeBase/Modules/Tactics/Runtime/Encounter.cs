using System;
using System.Collections.Generic;
using System.Linq;

namespace PetProject.Tactics
{
    /// <summary>Single authority for a prototype encounter. Only accepted commands mutate combat state.</summary>
    public sealed class Encounter
    {
        private sealed class ActorState
        {
            public readonly ActorDefinition Definition;
            public GridPosition Position;
            public int Health, ActionPoints;
            public long Initiative;
            public bool IsAlive => Health > 0;
            public ActorState(ActorDefinition definition)
            {
                Definition = definition;
                Position = definition.Position;
                Health = definition.MaxHealth;
            }
            public ActorSnapshot Snapshot() => new ActorSnapshot(Definition, Position, Health, ActionPoints, Initiative);
        }

        private readonly Dictionary<string, ActorState> _actors = new Dictionary<string, ActorState>(StringComparer.Ordinal);
        private readonly List<ActorState> _order;
        private readonly Dictionary<string, (TacticalCommand command, CommandResult result)> _accepted =
            new Dictionary<string, (TacticalCommand, CommandResult)>(StringComparer.Ordinal);
        private readonly IRandomSource _random;
        private int _turnIndex;
        private long _revision;
        private int _round = 1;
        private string _winner;
        public GridBoard Board { get; }
        public EncounterRules Rules { get; }
        public string CurrentActorId => _winner == null ? _order[_turnIndex].Definition.Id : null;

        public Encounter(GridBoard board, IEnumerable<ActorDefinition> actors, IRandomSource random, EncounterRules rules = null)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Rules = rules ?? new EncounterRules();
            if (actors == null) throw new ArgumentNullException(nameof(actors));
            var occupied = new HashSet<GridPosition>();
            var factions = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in actors)
            {
                if (definition == null || _actors.ContainsKey(definition.Id) || !board.Contains(definition.Position) ||
                    !board.GetCell(definition.Position).Walkable || !occupied.Add(definition.Position))
                    throw new ArgumentException("Actors need unique IDs and distinct walkable spawn cells", nameof(actors));
                _actors.Add(definition.Id, new ActorState(definition));
                factions.Add(definition.FactionId);
            }
            if (factions.Count < 2) throw new ArgumentException("An encounter needs at least two factions", nameof(actors));
            // Canonical input order makes RNG assignment independent of scene enumeration order.
            foreach (var actor in _actors.Values.OrderBy(a => a.Definition.Id, StringComparer.Ordinal))
                actor.Initiative = (long)RollD20() + actor.Definition.InitiativeBonus;
            _order = _actors.Values.OrderByDescending(a => a.Initiative)
                .ThenBy(a => a.Definition.Id, StringComparer.Ordinal).ToList();
            _order[0].ActionPoints = Rules.ActionsPerTurn;
        }

        public EncounterSnapshot GetSnapshot() => new EncounterSnapshot(_revision, _round, CurrentActorId, _winner,
            _random.State, _order.Select(a => a.Snapshot()).ToArray());

        public GridPath PreviewMove(string actorId, GridPosition destination)
        {
            if (!_actors.TryGetValue(actorId, out var actor) || !actor.IsAlive) return GridPath.Unreachable;
            return GridPathfinder.FindPath(Board, actor.Position, destination, OccupiedCells(), Rules.MaxHeightStep);
        }

        public CommandResult Execute(TacticalCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_accepted.TryGetValue(command.Id, out var previous))
                return previous.command.Equals(command) ? previous.result.AsReplay() : Reject(CommandError.CommandIdConflict);
            if (_winner != null) return Reject(CommandError.EncounterCompleted);
            if (!_actors.TryGetValue(command.ActorId, out var actor)) return Reject(CommandError.UnknownActor);
            if (!actor.IsAlive) return Reject(CommandError.ActorDefeated);
            if (CurrentActorId != command.ActorId) return Reject(CommandError.WrongTurn);

            var events = new List<TacticalEvent>();
            CommandError error;
            switch (command.Kind)
            {
                case TacticalCommandKind.Move: error = Move(actor, command.Destination, events); break;
                case TacticalCommandKind.Attack: error = Attack(actor, command.TargetId, events); break;
                case TacticalCommandKind.EndTurn: EndTurn(events); error = CommandError.None; break;
                default: throw new ArgumentOutOfRangeException(nameof(command));
            }
            if (error != CommandError.None) return Reject(error);
            var result = new CommandResult(CommandError.None, ++_revision, events.ToArray());
            _accepted.Add(command.Id, (command, result));
            return result;
        }

        private CommandError Move(ActorState actor, GridPosition destination, List<TacticalEvent> events)
        {
            if (!Board.Contains(destination)) return CommandError.InvalidDestination;
            if (actor.Position == destination) return CommandError.AlreadyAtDestination;
            var path = PreviewMove(actor.Definition.Id, destination);
            if (!path.Found) return CommandError.NoPath;
            if (path.Cost > actor.ActionPoints) return CommandError.NotEnoughActions;
            actor.Position = destination;
            actor.ActionPoints -= (int)path.Cost;
            events.Add(new TacticalEvent(TacticalEventKind.Moved, actor.Definition.Id, position: destination, amount: (int)path.Cost));
            return CommandError.None;
        }

        private CommandError Attack(ActorState actor, string targetId, List<TacticalEvent> events)
        {
            if (!_actors.TryGetValue(targetId, out var target)) return CommandError.UnknownTarget;
            if (!target.IsAlive) return CommandError.TargetDefeated;
            if (target.Definition.FactionId == actor.Definition.FactionId) return CommandError.FriendlyTarget;
            if (actor.Position.DistanceTo(target.Position) != 1 ||
                Math.Abs((long)Board.GetCell(actor.Position).Height - Board.GetCell(target.Position).Height) > Rules.MaxHeightStep)
                return CommandError.TargetOutOfRange;
            if (actor.ActionPoints < Rules.AttackCost) return CommandError.NotEnoughActions;
            var roll = RollD20();
            var hit = (long)roll + actor.Definition.AttackBonus >= target.Definition.Defense;
            var damage = hit ? Math.Min(target.Health, actor.Definition.Damage) : 0;
            actor.ActionPoints -= Rules.AttackCost;
            target.Health -= damage;
            events.Add(new TacticalEvent(hit ? TacticalEventKind.AttackHit : TacticalEventKind.AttackMiss,
                actor.Definition.Id, targetId, target.Position, damage, roll));
            if (!target.IsAlive)
            {
                target.ActionPoints = 0;
                events.Add(new TacticalEvent(TacticalEventKind.ActorDefeated, targetId));
                var aliveFactions = _actors.Values.Where(a => a.IsAlive).Select(a => a.Definition.FactionId)
                    .Distinct(StringComparer.Ordinal).ToArray();
                if (aliveFactions.Length == 1)
                {
                    _winner = aliveFactions[0];
                    events.Add(new TacticalEvent(TacticalEventKind.EncounterCompleted, actor.Definition.Id));
                }
            }
            return CommandError.None;
        }

        private void EndTurn(List<TacticalEvent> events)
        {
            var current = _order[_turnIndex];
            current.ActionPoints = 0;
            events.Add(new TacticalEvent(TacticalEventKind.TurnEnded, current.Definition.Id));
            do
            {
                _turnIndex = (_turnIndex + 1) % _order.Count;
                if (_turnIndex == 0) ++_round;
            } while (!_order[_turnIndex].IsAlive);
            var next = _order[_turnIndex];
            next.ActionPoints = Rules.ActionsPerTurn;
            events.Add(new TacticalEvent(TacticalEventKind.TurnStarted, next.Definition.Id, amount: next.ActionPoints));
        }

        private IEnumerable<GridPosition> OccupiedCells() => _actors.Values.Where(a => a.IsAlive).Select(a => a.Position);
        private CommandResult Reject(CommandError error) => new CommandResult(error, _revision, Array.Empty<TacticalEvent>());
        private int RollD20()
        {
            var roll = _random.NextInt(1, 21);
            if (roll < 1 || roll > 20) throw new InvalidOperationException("Random source returned an invalid d20 roll");
            return roll;
        }
    }
}
