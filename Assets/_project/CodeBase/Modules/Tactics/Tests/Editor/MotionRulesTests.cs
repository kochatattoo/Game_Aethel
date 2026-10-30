using System;
using NUnit.Framework;

namespace PetProject.Tactics.Tests
{
    public sealed class MotionRulesTests
    {
        [Test] public void WallBetweenWalkableCellsRequiresDetour()
        {
            var a = new GridPosition(0, 0); var b = new GridPosition(1, 0);
            var board = new GridBoard(3, 2, null, new[] { new GridTransition(a, b), new GridTransition(b, a) });
            var path = GridPathfinder.FindPath(board, a, b);
            Assert.That(path.Cost, Is.EqualTo(3));
            Assert.That(board.CanTraverse(a, b, 1), Is.False);
        }
        [Test] public void DirectedEdgeDoesNotBlockReverseStep()
        {
            var a = new GridPosition(0, 0); var b = new GridPosition(1, 0);
            var board = new GridBoard(2, 1, null, new[] { new GridTransition(a, b) });
            Assert.That(GridPathfinder.FindPath(board, a, b).Found, Is.False);
            Assert.That(GridPathfinder.FindPath(board, b, a).Found, Is.True);
        }
        [Test] public void EdgeDataIsCopiedAndInvalidEdgesAreRejected()
        {
            var edges = new[] { new GridTransition(new GridPosition(0, 0), new GridPosition(1, 0)) };
            var board = new GridBoard(2, 1, null, edges);
            edges[0] = default;
            Assert.That(board.CanTraverse(new GridPosition(0, 0), new GridPosition(1, 0), 1), Is.False);
            Assert.Throws<ArgumentException>(() => new GridBoard(2, 1, null, new[] { default(GridTransition) }));
        }
        [Test] public void PreviewHasNoMutationAndMoveChargesWeightedCells()
        {
            var board = new GridBoard(3, 1, new[] { new GridCell(new GridPosition(1, 0), moveCost: 2) });
            var session = new MotionTestSession(board, new GridPosition(0, 0), 6);
            Assert.That(session.Preview(new GridPosition(2, 0)).Cost, Is.EqualTo(3));
            Assert.That(session.Actions, Is.EqualTo(6)); Assert.That(session.Revision, Is.Zero);
            Assert.That(session.Move("move", new GridPosition(2, 0), 0).Accepted, Is.True);
            Assert.That(session.Actions, Is.EqualTo(3)); Assert.That(session.Revision, Is.EqualTo(1));
        }
        [Test] public void OverBudgetAndUnreachableMovesAreAtomic()
        {
            var session = new MotionTestSession(new GridBoard(4, 1), new GridPosition(0, 0), 1);
            Assert.That(session.Move("far", new GridPosition(3, 0), 0).Error, Is.EqualTo("NotEnoughActions"));
            Assert.That(session.Move("outside", new GridPosition(-1, 0), 0).Accepted, Is.False);
            Assert.That(session.Actions, Is.EqualTo(1)); Assert.That(session.Position, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.Revision, Is.Zero);
        }
        [Test] public void RetryAndConflictingIdDoNotSpendTwice()
        {
            var session = new MotionTestSession(new GridBoard(4, 1), new GridPosition(0, 0), 6);
            session.Move("one", new GridPosition(1, 0), 0);
            Assert.That(session.Move("one", new GridPosition(1, 0), 0).IsReplay, Is.True);
            Assert.That(session.Move("one", new GridPosition(2, 0), 0).Error, Is.EqualTo("CommandIdConflict"));
            Assert.That(session.Actions, Is.EqualTo(5)); Assert.That(session.Revision, Is.EqualTo(1));
        }
        [Test] public void NewTurnInvalidatesOldPreview()
        {
            var session = new MotionTestSession(new GridBoard(4, 1), new GridPosition(0, 0), 6);
            session.NewTurn();
            Assert.That(session.Move("stale", new GridPosition(1, 0), 0).Error, Is.EqualTo("StalePreview"));
            Assert.That(session.Actions, Is.EqualTo(6)); Assert.That(session.Revision, Is.EqualTo(1));
        }
    }
}
