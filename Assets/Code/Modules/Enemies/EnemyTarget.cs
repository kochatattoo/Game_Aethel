using UnityEngine;
using PetProject.Encounter;

namespace CodeBase.Enemies
{
    public sealed class EnemyTarget : MonoBehaviour
    {
        public string ActorId { get; private set; } = EncounterActorIds.DefaultEnemy;
        public string GroupId { get; private set; } = "skeleton-pack";
        public EnemyArchetypeConfig Archetype { get; private set; }

        public void Initialize(string actorId, string groupId, EnemyArchetypeConfig archetype = null)
        {
            ActorId = actorId;
            GroupId = groupId;
            Archetype = archetype;
        }
    }
}
