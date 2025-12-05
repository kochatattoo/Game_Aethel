using System;
using UnityEngine;


namespace CodeBase.Data
{
    [Serializable]
    public class LootObject
    {
        public string id;
        public Vector3Data positionData;
        public Loot loot;

        public LootObject(string Id, Vector3 position, Loot loot)
        {
            id = Id;
            positionData = position.AsVectorData();
            this.loot = loot;
        }
    }
}
