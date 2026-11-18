using UnityEngine;

namespace CodeBase.MotionTest
{
    public sealed class MotionEnemyTarget : MonoBehaviour
    {
        public const string SkeletonActorId = "skeleton";
        public string ActorId { get; private set; } = SkeletonActorId;
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
