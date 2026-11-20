using System;
using NUnit.Framework;
using PetProject.Encounter;
using PetProject.Navigation;

namespace PetProject.Tactics.Tests
{
    public sealed class EncounterCoreTests
    {
        [Test]
        [Author(Metadata.Author.Kostya)]
        [Category(Metadata.Category.Self)]
        public void StateMachineRejectsCombatWithoutAlignment()
        {
            var machine = new EncounterStateMachine();
            Assert.Throws<InvalidOperationException>(() => machine.TransitionTo(EncounterState.Combat));
            machine.TransitionTo(EncounterState.Following);
            machine.TransitionTo(EncounterState.Aligning);
            machine.TransitionTo(EncounterState.Combat);
            Assert.That(machine.State, Is.EqualTo(EncounterState.Combat));
        }

        [Test]
        [Author(Metadata.Author.Kostya)]
        [Category(Metadata.Category.Self)]
        public void GroupResolverFiltersGroupDeathAndRadius()
        {
            var members = new[]
            {
                new EncounterGroupMember("near", "pack", new NavigationPoint(1, 0, 0), true),
                new EncounterGroupMember("far", "pack", new NavigationPoint(20, 0, 0), true),
                new EncounterGroupMember("dead", "pack", new NavigationPoint(2, 0, 0), false),
                new EncounterGroupMember("other", "other", new NavigationPoint(1, 0, 0), true)
            };
            var result = EncounterGroupResolver.Resolve(members, "pack", default, 10);
            Assert.That(result, Is.EqualTo(new[] { "near" }));
        }

        [Test]
        [Author(Metadata.Author.Kostya)]
        [Category(Metadata.Category.Self)]
        public void PlannerAttacksAdjacentTarget()
        {
            var encounter = CreateEncounter(new GridPosition(0, 0), new GridPosition(1, 0));
            var command = new MeleeTurnPlanner(encounter, encounter.GetSnapshot()).CreateCommand(() => "attack");
            Assert.That(command.Kind, Is.EqualTo(TacticalCommandKind.Attack));
            Assert.That(command.TargetId, Is.EqualTo("hero"));
        }

        [Test]
        [Author(Metadata.Author.Kostya)]
        [Category(Metadata.Category.Self)]
        public void PlannerApproachesDistantTarget()
        {
            var encounter = CreateEncounter(new GridPosition(0, 0), new GridPosition(3, 0));
            var command = new MeleeTurnPlanner(encounter, encounter.GetSnapshot()).CreateCommand(() => "move");
            Assert.That(command.Kind, Is.EqualTo(TacticalCommandKind.Move));
            Assert.That(command.Destination, Is.EqualTo(new GridPosition(1, 0)));
        }

        private static Encounter CreateEncounter(GridPosition heroPosition, GridPosition enemyPosition) =>
            new Encounter(new GridBoard(5, 1), new[]
            {
                new ActorDefinition(EncounterActorIds.Hero, EncounterActorIds.AlliesFaction, heroPosition),
                new ActorDefinition("enemy", EncounterActorIds.EnemiesFaction, enemyPosition)
            }, new DeterministicRandom(718), openingActorId: "enemy");
    }
}
