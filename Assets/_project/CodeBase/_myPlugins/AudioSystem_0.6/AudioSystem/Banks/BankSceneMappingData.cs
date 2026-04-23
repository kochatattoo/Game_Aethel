using Infrastructure.AudioSystem.Events;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AudioSystem.Banks
{
    [Serializable]
    public class BankSceneMappingData
    {
        [field: SerializeField]
        public SceneAsset SceneAsset { get; private set; }

        [field: SerializeField] 
        public string SceneName { get; private set; }

        [field: SerializeField]
        public List<WwiseBankAsset> BankSceneMapping { get; private set; } = new();

#if UNITY_EDITOR
        public void OnValidate()
        {
            SceneName = SceneAsset != null ? SceneAsset.name : string.Empty;
        }
#endif
    }
}
