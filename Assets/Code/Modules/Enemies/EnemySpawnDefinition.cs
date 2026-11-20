using System;
using UnityEngine;

namespace CodeBase.Enemies
{
    [Serializable]
    public sealed class EnemySpawnDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private Vector3 _position;
        [SerializeField] private string _groupId = "skeleton-pack";
        [SerializeField] private EnemyArchetypeConfig _archetype;

        public string Id => _id;
        public Vector3 Position => _position;
        public string GroupId => _groupId;
        public EnemyArchetypeConfig Archetype => _archetype;

        public EnemySpawnDefinition()
        {
        }

        public EnemySpawnDefinition(Vector3 position, string groupId, EnemyArchetypeConfig archetype = null,
            string id = null)
        {
            _position = position;
            _groupId = groupId;
            _archetype = archetype;
            _id = id;
        }
    }
}
