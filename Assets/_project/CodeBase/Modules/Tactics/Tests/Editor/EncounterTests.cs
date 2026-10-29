using System;
using System.Linq;
using NUnit.Framework;

namespace PetProject.Tactics.Tests
{
    public sealed class EncounterTests
    {
        private static ActorDefinition Hero(GridPosition position, int attackBonus = 3) =>
            new ActorDefinition("hero", "allies", position, initiativeBonus: 20, attackBonus: attackBonus);
        private static ActorDefinition Enemy(GridPosition position, int health = 12, string id = "enemy", int initiative = 0) =>
            new ActorDefinition(id, "enemies", position, health, initiative);
        private static Encounter Create(params ActorDefinition[] actors) =>
            new Encounter(new GridBoard(8, 5), actors, new DeterministicRandom(718));
        private static ActorSnapshot Actor(Encounter encounter, string id) => encounter.GetSnapshot().Actors.Single(a => a.Id == id);
        private static string Fingerprint(Encounter encounter)
        {
            var s = encounter.GetSnapshot();
            return $"{s.Revision}/{s.Round}/{s.CurrentActorId}/{s.WinningFactionId}/{s.RandomState}/" +
                string.Join(";", s.Actors.Select(a => $"{a.Id}:{a.Position}:{a.Health}:{a.ActionPoints}:{a.Initiative}"));
        }
        private static void RejectsWithoutMutation(Encounter encounter, TacticalCommand command, CommandError error)
        {
            var before = Fingerprint(encounter);
            Assert.That(encounter.Execute(command).Error, Is.EqualTo(error));
            Assert.That(Fingerprint(encounter), Is.EqualTo(before));
        }

        [Test] public void SceneEnumerationDoesNotChangeInitiativeOrRandomStream()
        {
            var a = Hero(new GridPosition(0, 0)); var b = Enemy(new GridPosition(4, 0));
            Assert.That(Fingerprint(Create(a, b)), Is.EqualTo(Fingerprint(Create(b, a))));
        }

        [Test] public void OnlyCurrentActorCanAct()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            RejectsWithoutMutation(e, TacticalCommand.Move("x", "enemy", new GridPosition(3, 0)), CommandError.WrongTurn);
            RejectsWithoutMutation(e, TacticalCommand.EndTurn("y", "unknown"), CommandError.UnknownActor);
        }

        [Test] public void MovementChargesEntirePathAndLeavesOldSnapshotImmutable()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            var old = e.GetSnapshot();
            Assert.That(e.Execute(TacticalCommand.Move("move", "hero", new GridPosition(2, 0))).Accepted, Is.True);
            Assert.That(Actor(e, "hero").ActionPoints, Is.EqualTo(1));
            Assert.That(Actor(e, "hero").Position, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(old.Actors.Single(a => a.Id == "hero").Position, Is.EqualTo(new GridPosition(0, 0)));
        }

        [Test] public void ImpossibleAndUnaffordableMovesDoNotChangeState()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            RejectsWithoutMutation(e, TacticalCommand.Move("far", "hero", new GridPosition(0, 4)), CommandError.NotEnoughActions);
            RejectsWithoutMutation(e, TacticalCommand.Move("occupied", "hero", new GridPosition(4, 0)), CommandError.NoPath);
            RejectsWithoutMutation(e, TacticalCommand.Move("outside", "hero", new GridPosition(-1, 0)), CommandError.InvalidDestination);
            RejectsWithoutMutation(e, TacticalCommand.Move("same", "hero", new GridPosition(0, 0)), CommandError.AlreadyAtDestination);
        }

        [Test] public void AcceptedCommandRetryHasNoEventsOrSecondCost()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            var move = TacticalCommand.Move("once", "hero", new GridPosition(1, 0));
            Assert.That(e.Execute(move).Accepted, Is.True); var before = Fingerprint(e);
            var replay = e.Execute(move);
            Assert.That(replay.Accepted && replay.IsReplay, Is.True);
            Assert.That(replay.Events, Is.Empty);
            Assert.That(Fingerprint(e), Is.EqualTo(before));
        }

        [Test] public void ReusedCommandIdWithDifferentPayloadIsRejected()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            e.Execute(TacticalCommand.Move("once", "hero", new GridPosition(1, 0)));
            RejectsWithoutMutation(e, TacticalCommand.EndTurn("once", "hero"), CommandError.CommandIdConflict);
        }

        [Test] public void HitClampsHealthAndCompletesEncounter()
        {
            var e = Create(Hero(new GridPosition(0, 0), 100), Enemy(new GridPosition(1, 0), 2));
            var command = TacticalCommand.Attack("attack", "hero", "enemy");
            var result = e.Execute(command);
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events.Single(x => x.Kind == TacticalEventKind.AttackHit).Amount, Is.EqualTo(2));
            Assert.That(Actor(e, "enemy").Health, Is.Zero);
            Assert.That(e.GetSnapshot().WinningFactionId, Is.EqualTo("allies"));
            Assert.That(e.CurrentActorId, Is.Null);
            var before = Fingerprint(e);
            Assert.That(e.Execute(command).IsReplay, Is.True);
            Assert.That(Fingerprint(e), Is.EqualTo(before));
            RejectsWithoutMutation(e, TacticalCommand.EndTurn("after", "hero"), CommandError.EncounterCompleted);
        }

        [Test] public void MissStillCostsActionsAndConsumesOneD20()
        {
            var e = Create(Hero(new GridPosition(0, 0), -100), Enemy(new GridPosition(1, 0)));
            var before = e.GetSnapshot(); var result = e.Execute(TacticalCommand.Attack("miss", "hero", "enemy"));
            Assert.That(result.Events.Single().Kind, Is.EqualTo(TacticalEventKind.AttackMiss));
            Assert.That(result.Events.Single().Roll, Is.InRange(1, 20));
            Assert.That(Actor(e, "enemy").Health, Is.EqualTo(12));
            Assert.That(Actor(e, "hero").ActionPoints, Is.EqualTo(2));
            Assert.That(e.GetSnapshot().RandomState, Is.Not.EqualTo(before.RandomState));
        }

        [Test] public void InvalidAttacksDoNotSpendActionsOrRandomness()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(1, 1)));
            RejectsWithoutMutation(e, TacticalCommand.Attack("far", "hero", "enemy"), CommandError.TargetOutOfRange);
            RejectsWithoutMutation(e, TacticalCommand.Attack("friend", "hero", "hero"), CommandError.FriendlyTarget);
            RejectsWithoutMutation(e, TacticalCommand.Attack("unknown", "hero", "other"), CommandError.UnknownTarget);
        }

        [Test] public void ZeroActionsDoesNotAutomaticallyAdvanceTurn()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            e.Execute(TacticalCommand.Move("move", "hero", new GridPosition(3, 0)));
            Assert.That(Actor(e, "hero").ActionPoints, Is.Zero); Assert.That(e.CurrentActorId, Is.EqualTo("hero"));
            RejectsWithoutMutation(e, TacticalCommand.Attack("attack", "hero", "enemy"), CommandError.NotEnoughActions);
            Assert.That(e.Execute(TacticalCommand.EndTurn("end", "hero")).Accepted, Is.True);
            Assert.That(e.CurrentActorId, Is.EqualTo("enemy")); Assert.That(Actor(e, "enemy").ActionPoints, Is.EqualTo(3));
        }

        [Test] public void RoundAdvancesAfterEveryLivingParticipant()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0)));
            e.Execute(TacticalCommand.EndTurn("a", "hero")); e.Execute(TacticalCommand.EndTurn("b", "enemy"));
            Assert.That(e.GetSnapshot().Round, Is.EqualTo(2)); Assert.That(e.CurrentActorId, Is.EqualTo("hero"));
            Assert.That(Actor(e, "hero").ActionPoints, Is.EqualTo(3));
        }

        [Test] public void DefeatedActorsDoNotOccupyCellsOrReceiveTurns()
        {
            var e = Create(Hero(new GridPosition(0, 0), 100), Enemy(new GridPosition(1, 0), 4),
                Enemy(new GridPosition(4, 0), id: "last", initiative: -100));
            e.Execute(TacticalCommand.Attack("kill", "hero", "enemy"));
            RejectsWithoutMutation(e, TacticalCommand.Attack("again", "hero", "enemy"), CommandError.TargetDefeated);
            Assert.That(e.Execute(TacticalCommand.Move("step", "hero", new GridPosition(1, 0))).Accepted, Is.True);
            e.Execute(TacticalCommand.EndTurn("end", "hero")); Assert.That(e.CurrentActorId, Is.EqualTo("last"));
        }

        [Test] public void ReadingSnapshotNeverAdvancesSimulation()
        {
            var e = Create(Hero(new GridPosition(0, 0)), Enemy(new GridPosition(4, 0))); var before = Fingerprint(e);
            for (var i = 0; i < 100; ++i) e.GetSnapshot();
            Assert.That(Fingerprint(e), Is.EqualTo(before));
        }

        [Test] public void InvalidRosterAndRulesAreRejected()
        {
            var a = Hero(new GridPosition(0, 0));
            Assert.Throws<ArgumentException>(() => Create(a, a));
            Assert.Throws<ArgumentException>(() => Create(a, Enemy(new GridPosition(0, 0))));
            Assert.Throws<ArgumentException>(() => Create(a));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EncounterRules(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EncounterRules(3, 4));
        }
    }
}
