using System;
using UnityEngine;

namespace CodeBase.MotionTest
{
    [Serializable]
    public sealed class SkeletonEnemySpawn
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private string _groupId = "skeleton-pack";

        public Vector3 Position => _position;
        public string GroupId => _groupId;

        public SkeletonEnemySpawn()
        {
        }

        public SkeletonEnemySpawn(Vector3 position, string groupId)
        {
            _position = position;
            _groupId = groupId;
        }
    }
}
