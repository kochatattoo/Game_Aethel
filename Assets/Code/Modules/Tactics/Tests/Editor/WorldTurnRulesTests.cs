using System;
using NUnit.Framework;

namespace PetProject.Tactics.Tests
{
    public sealed class WorldTurnRulesTests
    {
        [Test] public void BoundaryResetsAllSavedBudgets()
        {
            var clock = new WorldTurnClock();
            var budgets = new WorldActionBudgets();
            budgets.Set("hero", 4, 6);
            budgets.Set("npc", 2, 3);
            Assert.That(clock.Advance(5.9), Is.Zero);
            Assert.That(budgets.Get("hero", 6), Is.EqualTo(4));
            Assert.That(clock.Advance(.1), Is.EqualTo(1));
            budgets.ResetAll();
            Assert.That(budgets.Get("hero", 6), Is.EqualTo(6));
            Assert.That(budgets.Get("npc", 3), Is.EqualTo(3));
        }

        [Test] public void MultipleRoundsPreserveRemainder()
        {
            var clock = new WorldTurnClock();
            Assert.That(clock.Advance(13.25), Is.EqualTo(2));
            Assert.That(clock.Round, Is.EqualTo(2));
            Assert.That(clock.SecondsToNextRound, Is.EqualTo(4.75).Within(1e-9));
        }

        [Test] public void NoAdvanceMeansNoWorldRestoration()
        {
            var clock = new WorldTurnClock();
            var budgets = new WorldActionBudgets();
            budgets.Set("hero", 1, 6);
            Assert.That(clock.Advance(0), Is.Zero);
            Assert.That(clock.Round, Is.Zero);
            Assert.That(budgets.Get("hero", 6), Is.EqualTo(1));
        }

        [Test] public void MissingActorsStartWithTheirOwnFullBudget()
        {
            var budgets = new WorldActionBudgets();
            Assert.That(budgets.Get("hero", 6), Is.EqualTo(6));
            Assert.That(budgets.Get("npc", 3), Is.EqualTo(3));
        }

        [Test] public void ResetRestartsTheWorldRound()
        {
            var clock = new WorldTurnClock();
            clock.Advance(8);
            clock.Reset();
            Assert.That(clock.Round, Is.Zero);
            Assert.That(clock.SecondsToNextRound, Is.EqualTo(6));
        }

        [Test] public void InvalidTimeAndBudgetsAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorldTurnClock(0));
            var clock = new WorldTurnClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(double.NaN));
            var budgets = new WorldActionBudgets();
            Assert.Throws<ArgumentOutOfRangeException>(() => budgets.Set("hero", 7, 6));
            Assert.Throws<ArgumentException>(() => budgets.Get("", 6));
        }
    }
}
