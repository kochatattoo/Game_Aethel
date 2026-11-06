using System;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.BehaviourTreeExtansions;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Factories;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Leaf;
using PetProject.Tactics;

namespace CodeBase.MotionTest
{
    internal sealed class SkeletonMeleeBehaviourFactory : BehaviourFactory
    {
        private readonly Encounter _encounter;
        private readonly EncounterSnapshot _snapshot;
        private readonly MeleeTacticsAIConfig _config;
        private readonly Func<string> _nextId;
        private GridPath _approachPath;

        public TacticalCommand Command { get; private set; }

        public SkeletonMeleeBehaviourFactory(Encounter encounter, EncounterSnapshot snapshot,
            MeleeTacticsAIConfig config, Func<string> nextId)
        {
            _encounter = encounter;
            _snapshot = snapshot;
            _config = config;
            _nextId = nextId;
        }

        protected override BehaviourNode CreateRoot() =>
            PrioritySelector("Skeleton melee tactics")
                .Add(Sequence("Attack if adjacent", AttackPriority)
                    .Add(Leaf("Can attack", new Condition(CanAttack)))
                    .Add(Leaf("Attack", new ActionStrategy(SetAttack))))
                .Add(Sequence("Approach hero", MovePriority)
                    .Add(Leaf("Has approach path", new Condition(FindApproachPath)))
                    .Add(Leaf("Move", new ActionStrategy(SetMove))))
                .Add(Leaf("End turn", new ActionStrategy(SetEndTurn), EndTurnPriority));

        private bool CanAttack()
        {
            var enemy = Enemy;
            var hero = Hero;
            return enemy.ActionPoints >= _encounter.Rules.AttackCost &&
                enemy.Position.DistanceTo(hero.Position) == 1 &&
                Math.Abs((long)_encounter.Board.GetCell(enemy.Position).Height -
                    _encounter.Board.GetCell(hero.Position).Height) <= _encounter.Rules.MaxHeightStep;
        }

        private bool FindApproachPath()
        {
            GridPath best = null;
            var hero = Hero.Position;
            var candidates = new[]
            {
                new GridPosition(hero.X - 1, hero.Y),
                new GridPosition(hero.X + 1, hero.Y),
                new GridPosition(hero.X, hero.Y - 1),
                new GridPosition(hero.X, hero.Y + 1)
            };
            foreach (var candidate in candidates)
            {
                if (!_encounter.Board.Contains(candidate) || !_encounter.Board.GetCell(candidate).Walkable) continue;
                var path = _encounter.PreviewMove(Enemy.Id, candidate);
                if (path.Found && (best == null || path.Cost < best.Cost)) best = path;
            }

            var movementBudget = Math.Min(Enemy.ActionPoints,
                _config != null ? _config.MaxMovementCostPerCommand : Enemy.ActionPoints);
            _approachPath = LimitPath(best, movementBudget);
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
            return destination == path.Cells[0] ? null : _encounter.PreviewMove(Enemy.Id, destination);
        }

        private void SetAttack() => Command = TacticalCommand.Attack(_nextId(), Enemy.Id, Hero.Id);
        private void SetMove() => Command = TacticalCommand.Move(_nextId(), Enemy.Id,
            _approachPath.Cells[_approachPath.Cells.Count - 1]);
        private void SetEndTurn() => Command = TacticalCommand.EndTurn(_nextId(), Enemy.Id);

        private ActorSnapshot Enemy => _snapshot.GetActor(MotionEnemyTarget.SkeletonActorId);
        private ActorSnapshot Hero => _snapshot.GetActor(MotionTestController.HeroActorId);
        private int AttackPriority => _config != null ? _config.AttackPriority : 100;
        private int MovePriority => _config != null ? _config.MovePriority : 50;
        private int EndTurnPriority => _config != null ? _config.EndTurnPriority : 0;
    }
}
