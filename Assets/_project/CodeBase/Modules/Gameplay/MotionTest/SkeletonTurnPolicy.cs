using System;
using PetProject.Tactics;

namespace CodeBase.MotionTest
{
    internal static class SkeletonTurnPolicy
    {
        public static TacticalCommand CreateCommand(Encounter encounter, EncounterSnapshot snapshot,
            Func<string> nextId)
        {
            var enemy = snapshot.GetActor(MotionEnemyTarget.SkeletonActorId);
            var hero = snapshot.GetActor(MotionTestController.HeroActorId);
            if (enemy.Position.DistanceTo(hero.Position) == 1)
                return TacticalCommand.Attack(nextId(), enemy.Id, hero.Id);

            GridPath best = null;
            var candidates = new[]
            {
                new GridPosition(hero.Position.X - 1, hero.Position.Y),
                new GridPosition(hero.Position.X + 1, hero.Position.Y),
                new GridPosition(hero.Position.X, hero.Position.Y - 1),
                new GridPosition(hero.Position.X, hero.Position.Y + 1)
            };
            foreach (var candidate in candidates)
            {
                if (!encounter.Board.Contains(candidate) || !encounter.Board.GetCell(candidate).Walkable) continue;
                var path = encounter.PreviewMove(enemy.Id, candidate);
                if (!path.Found) continue;
                if (best == null || path.Cost < best.Cost) best = path;
            }

            best = LimitPath(encounter, best, enemy.ActionPoints);
            return best != null && best.Cells.Count > 1
                ? TacticalCommand.Move(nextId(), enemy.Id, best.Cells[best.Cells.Count - 1])
                : TacticalCommand.EndTurn(nextId(), enemy.Id);
        }

        private static GridPath LimitPath(Encounter encounter, GridPath path, int actions)
        {
            if (path == null || !path.Found) return null;
            if (path.Cost <= actions) return path;
            var cost = 0;
            var destination = path.Cells[0];
            for (var i = 1; i < path.Cells.Count; ++i)
            {
                cost += encounter.Board.GetCell(path.Cells[i]).MoveCost;
                if (cost > actions) break;
                destination = path.Cells[i];
            }
            return destination == path.Cells[0]
                ? null
                : encounter.PreviewMove(MotionEnemyTarget.SkeletonActorId, destination);
        }
    }
}
