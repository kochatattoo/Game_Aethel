using System;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using PetProject.Tactics;

namespace CodeBase.MotionTest
{
    internal sealed class SkeletonTurnPolicy
    {
        private readonly MeleeTacticsAIConfig _config;

        public SkeletonTurnPolicy(MeleeTacticsAIConfig config)
        {
            _config = config;
        }

        public TacticalCommand CreateCommand(Encounter encounter, EncounterSnapshot snapshot, Func<string> nextId)
        {
            var factory = new SkeletonMeleeBehaviourFactory(encounter, snapshot, _config, nextId);
            var tree = new BehaviourTree("Skeleton combat turn");
            tree.AddChild(factory.BuildTree());
            tree.Process();
            if (factory.Command == null) tree.Process();
            return factory.Command ?? TacticalCommand.EndTurn(nextId(), MotionEnemyTarget.SkeletonActorId);
        }
    }
}
