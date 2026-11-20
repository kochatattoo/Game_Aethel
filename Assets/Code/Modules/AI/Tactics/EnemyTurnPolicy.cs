using System;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using PetProject.Encounter;
using PetProject.Tactics;

namespace CodeBase.AI.Tactics
{
    public sealed class EnemyTurnPolicy
    {
        private readonly MeleeTacticsAIConfig _config;

        public EnemyTurnPolicy(MeleeTacticsAIConfig config)
        {
            _config = config;
        }

        public TacticalCommand CreateCommand(Encounter encounter, EncounterSnapshot snapshot, Func<string> nextId,
            MeleeTacticsAIConfig config = null)
        {
            var currentConfig = config ?? _config;
            var movementLimit = currentConfig != null ? currentConfig.MaxMovementCostPerCommand : int.MaxValue;
            return new MeleeTurnPlanner(encounter, snapshot, EncounterActorIds.AlliesFaction, movementLimit).CreateCommand(nextId);
        }
    }
}
