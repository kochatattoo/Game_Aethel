using System;
using UnityEngine;

namespace CodeBase.StaticData
{
    [Serializable]
    public class SaveTriggerData
    {
        public string Id;
        public Vector3 Position;

        public SaveTriggerData(string id, Vector3 position)
        {
            Id = id;
            Position = position;
        }
    }
}
