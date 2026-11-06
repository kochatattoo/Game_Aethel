using System.Collections.Generic;

namespace PetProject.Tactics
{
    /// <summary>Terrain/occupancy preview and atomic movement cost validation.</summary>
    internal sealed class MovementResolver
    {
        private readonly GridBoard _board;
        private readonly EncounterRules _rules;
        private readonly EncounterRoster _roster;

        public MovementResolver(GridBoard board, EncounterRules rules, EncounterRoster roster)
        {
            _board = board;
            _rules = rules;
            _roster = roster;
        }

        public GridPath Preview(string actorId, GridPosition destination)
        {
            if (!_roster.TryGet(actorId, out var actor) || !actor.IsAlive) return GridPath.Unreachable;
            return GridPathfinder.FindPath(_board, actor.Position, destination, _roster.OccupiedCells(), _rules.MaxHeightStep);
        }

        public CommandError TryMove(ActorState actor, GridPosition destination, List<TacticalEvent> events)
        {
            if (!_board.Contains(destination)) return CommandError.InvalidDestination;
            if (actor.Position == destination) return CommandError.AlreadyAtDestination;
            var path = Preview(actor.Definition.Id, destination);
            if (!path.Found) return CommandError.NoPath;
            if (path.Cost > actor.ActionPoints) return CommandError.NotEnoughActions;
            actor.MoveTo(destination, (int)path.Cost);
            events.Add(new TacticalEvent(TacticalEventKind.Moved, actor.Definition.Id, position: destination, amount: (int)path.Cost));
            return CommandError.None;
        }
    }
}
