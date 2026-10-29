using System;
using UnityEngine;

namespace CodeBase.StaticData
{
    [Serializable]
    public class EnemySpawnerData
    {
        public string Id;
        public MonsterTypeID MonsterTypeID;
        public Vector3 position;

        public EnemySpawnerData(string id, MonsterTypeID monsterTypeID, Vector3 position)
        {
            Id = id;
            MonsterTypeID = monsterTypeID;
            this.position = position;
        }
    }
}
