using System;
using PetProject.Tactics;
using TacticalEncounter = PetProject.Tactics.Encounter;

namespace PetProject.Encounter
{
    /// <summary>Выбор ближней атаки, сближения или завершения хода без Unity и BehaviourTree.</summary>
    public sealed class MeleeTurnPlanner
    {
        private readonly TacticalEncounter _encounter;
        private readonly EncounterSnapshot _snapshot;
        private readonly string _targetFactionId;
        private readonly int _maxMovementCost;
        private GridPath _approachPath;

        public MeleeTurnPlanner(TacticalEncounter encounter, EncounterSnapshot snapshot, string targetFactionId = EncounterActorIds.AlliesFaction,
            int maxMovementCost = int.MaxValue)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            _targetFactionId = targetFactionId;
            _maxMovementCost = Math.Max(0, maxMovementCost);
        }

        public TacticalCommand CreateCommand(Func<string> nextId)
        {
            if (nextId == null) throw new ArgumentNullException(nameof(nextId));
            var actor = CurrentActor;
            var target = FindTarget(actor);
            if (target == null) return TacticalCommand.EndTurn(nextId(), actor.Id);
            if (CanAttack(actor, target)) return TacticalCommand.Attack(nextId(), actor.Id, target.Id);
            if (FindApproachPath(actor, target))
                return TacticalCommand.Move(nextId(), actor.Id, _approachPath.Cells[_approachPath.Cells.Count - 1]);
            return TacticalCommand.EndTurn(nextId(), actor.Id);
        }

        private bool CanAttack(ActorSnapshot actor, ActorSnapshot target) =>
            actor.ActionPoints >= _encounter.Rules.AttackCost &&
            GridAdjacency.IsAdjacent(actor.Position, target.Position) &&
            Math.Abs((long)_encounter.Board.GetCell(actor.Position).Height -
                _encounter.Board.GetCell(target.Position).Height) <= _encounter.Rules.MaxHeightStep;

        private bool FindApproachPath(ActorSnapshot actor, ActorSnapshot target)
        {
            GridPath best = null;
            foreach (var candidate in GridAdjacency.GetNeighbors(target.Position))
            {
                if (!_encounter.Board.Contains(candidate) || !_encounter.Board.GetCell(candidate).Walkable) continue;
                var path = _encounter.PreviewMove(actor.Id, candidate);
                if (path.Found && (best == null || path.Cost < best.Cost)) best = path;
            }

            _approachPath = LimitPath(best, Math.Min(actor.ActionPoints, _maxMovementCost));
            return _approachPath != null && _approachPath.Cells.Count > 1;
        }

        private GridPath LimitPath(GridPath path, int actions)
        {
            if (path == null || !path.Found) return null;
            if (path.Cost <= actions) return path;
            var cost = 0;
            var destination = path.Cells[0];
            for (var i = 1; i < path.Cells.Count; ++i)
            {
                cost += _encounter.Board.GetCell(path.Cells[i]).MoveCost;
                if (cost > actions) break;
                destination = path.Cells[i];
            }
            return destination == path.Cells[0] ? null : _encounter.PreviewMove(CurrentActor.Id, destination);
        }

        private ActorSnapshot FindTarget(ActorSnapshot actor)
        {
            foreach (var candidate in _snapshot.Actors)
                if (candidate.IsAlive && candidate.FactionId == _targetFactionId && candidate.Id != actor.Id)
                    return candidate;
            return null;
        }

        private ActorSnapshot CurrentActor => _snapshot.GetActor(_snapshot.CurrentActorId);
    }
}
