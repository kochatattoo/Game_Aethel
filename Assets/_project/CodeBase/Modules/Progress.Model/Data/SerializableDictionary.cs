using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Utility
{
    [Serializable]
    public class SerializableDictionary<K, V> : ISerializationCallbackReceiver
    {
        [SerializeField] private List<K> keys = new List<K>();
        [SerializeField] private List<V> values = new List<V>();

        public Dictionary<K, V> Dict = new Dictionary<K, V>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in Dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Dict.Clear();
            int count = Mathf.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
                Dict[keys[i]] = values[i];
        }

    }
}
