using UnityEngine;

namespace CodeBase.MotionTest
{
    public sealed class MotionEnemyTarget : MonoBehaviour
    {
        public const string SkeletonActorId = "skeleton";
        public string ActorId { get; private set; } = SkeletonActorId;
        public string GroupId { get; private set; } = "skeleton-pack";

        public void Initialize(string actorId, string groupId)
        {
            ActorId = actorId;
            GroupId = groupId;
        }
    }
}
