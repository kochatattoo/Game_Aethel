using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    /// <summary>Single authority for a prototype encounter. Only accepted commands mutate combat state.</summary>
    public sealed class Encounter
    {
        private readonly EncounterRoster _roster;
        private readonly InitiativeOrder _turns;
        private readonly MovementResolver _movement;
        private readonly MeleeAttackResolver _attacks;
        private readonly Dictionary<string, (TacticalCommand command, CommandResult result)> _accepted =
            new Dictionary<string, (TacticalCommand, CommandResult)>(StringComparer.Ordinal);
        private readonly IRandomSource _random;
        private long _revision;
        public GridBoard Board { get; }
        public EncounterRules Rules { get; }
        public string CurrentActorId => _roster.WinningFactionId == null ? _turns.Current.Definition.Id : null;

        public Encounter(GridBoard board, IEnumerable<ActorDefinition> actors, IRandomSource random, EncounterRules rules = null)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Rules = rules ?? new EncounterRules();
            _roster = new EncounterRoster(board, actors);
            _turns = new InitiativeOrder(_roster.Actors, Rules.ActionsPerTurn, RollD20);
            _movement = new MovementResolver(board, Rules, _roster);
            _attacks = new MeleeAttackResolver(board, Rules, _roster, RollD20);
        }

        public EncounterSnapshot GetSnapshot() => new EncounterSnapshot(_revision, _turns.Round, CurrentActorId,
            _roster.WinningFactionId, _random.State, _turns.CaptureActors());

        public GridPath PreviewMove(string actorId, GridPosition destination) => _movement.Preview(actorId, destination);

        public CommandResult Execute(TacticalCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            // A completed encounter still acknowledges a retry of an accepted command.
            if (_accepted.TryGetValue(command.Id, out var previous))
                return previous.command.Equals(command) ? previous.result.AsReplay() : Reject(CommandError.CommandIdConflict);
            if (_roster.WinningFactionId != null) return Reject(CommandError.EncounterCompleted);
            if (!_roster.TryGet(command.ActorId, out var actor)) return Reject(CommandError.UnknownActor);
            if (!actor.IsAlive) return Reject(CommandError.ActorDefeated);
            if (CurrentActorId != command.ActorId) return Reject(CommandError.WrongTurn);

            var events = new List<TacticalEvent>();
            CommandError error;
            switch (command.Kind)
            {
                case TacticalCommandKind.Move: error = _movement.TryMove(actor, command.Destination, events); break;
                case TacticalCommandKind.Attack: error = _attacks.TryAttack(actor, command.TargetId, events); break;
                case TacticalCommandKind.EndTurn: _turns.EndTurn(events); error = CommandError.None; break;
                default: throw new ArgumentOutOfRangeException(nameof(command));
            }
            if (error != CommandError.None) return Reject(error);
            var result = new CommandResult(CommandError.None, ++_revision, events.ToArray());
            _accepted.Add(command.Id, (command, result));
            return result;
        }

        private CommandResult Reject(CommandError error) => new CommandResult(error, _revision, Array.Empty<TacticalEvent>());
        private int RollD20()
        {
            var roll = _random.NextInt(1, 21);
            if (roll < 1 || roll > 20) throw new InvalidOperationException("Random source returned an invalid d20 roll");
            return roll;
        }
    }
}
