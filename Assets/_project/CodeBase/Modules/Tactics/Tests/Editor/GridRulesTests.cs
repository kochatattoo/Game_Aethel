using System;
using System.Linq;
using NUnit.Framework;

namespace PetProject.Tactics.Tests
{
    public sealed class GridRulesTests
    {
        [Test] public void WeightedPathChoosesCheaperDetour()
        {
            var expensive = new GridPosition(1, 0);
            var board = new GridBoard(3, 2, new[] { new GridCell(expensive, moveCost: 9) });
            var path = GridPathfinder.FindPath(board, new GridPosition(0, 0), new GridPosition(2, 0));
            Assert.That(path.Found, Is.True);
            Assert.That(path.Cost, Is.EqualTo(4));
            Assert.That(path.Cells.Contains(expensive), Is.False);
        }

        [Test] public void OccupiedGoalAndBlockedColumnAreUnreachable()
        {
            var start = new GridPosition(0, 0); var goal = new GridPosition(2, 0);
            Assert.That(GridPathfinder.FindPath(new GridBoard(3, 2), start, goal, new[] { goal }).Found, Is.False);
            var board = new GridBoard(3, 2, new[]
            {
                new GridCell(new GridPosition(1, 0), false), new GridCell(new GridPosition(1, 1), false)
            });
            Assert.That(GridPathfinder.FindPath(board, start, goal).Found, Is.False);
        }

        [Test] public void HeightLimitIsAppliedWithoutIntegerOverflow()
        {
            var start = new GridPosition(0, 0); var goal = new GridPosition(1, 0);
            var board = new GridBoard(2, 1, new[] { new GridCell(goal, height: 2) });
            Assert.That(GridPathfinder.FindPath(board, start, goal, maxHeightStep: 1).Found, Is.False);
            Assert.That(GridPathfinder.FindPath(board, start, goal, maxHeightStep: 2).Found, Is.True);
            var extremes = new GridBoard(2, 1, new[]
            {
                new GridCell(start, height: int.MinValue), new GridCell(goal, height: int.MaxValue)
            });
            Assert.That(GridPathfinder.FindPath(extremes, start, goal, maxHeightStep: int.MaxValue).Found, Is.False);
        }

        [Test] public void DiagonalMovementUsesTwoCardinalSteps()
        {
            var path = GridPathfinder.FindPath(new GridBoard(2, 2), new GridPosition(0, 0), new GridPosition(1, 1));
            Assert.That(path.Cost, Is.EqualTo(2));
            Assert.That(path.Cells.Count, Is.EqualTo(3));
        }

        [Test] public void InvalidEndpointsReturnNoPathAndStartHasZeroCost()
        {
            var board = new GridBoard(2, 2); var start = new GridPosition(0, 0);
            Assert.That(GridPathfinder.FindPath(board, start, new GridPosition(-1, 0)).Found, Is.False);
            var path = GridPathfinder.FindPath(board, start, start, new[] { start });
            Assert.That(path.Found, Is.True); Assert.That(path.Cost, Is.Zero);
        }

        [Test] public void BoardRejectsAmbiguousOrInvalidTerrain()
        {
            var cell = new GridCell(new GridPosition(0, 0));
            Assert.Throws<ArgumentException>(() => new GridBoard(2, 2, new[] { cell, cell }));
            Assert.Throws<ArgumentException>(() => new GridBoard(2, 2, new[] { default(GridCell) }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridBoard(int.MaxValue, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridCell(new GridPosition(0, 0), moveCost: 0));
        }
    }
}
