using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    public class OcclusionAudioComponent : MonoBehaviour
    {
        [System.Serializable]
        public struct ParameterSetting
        {
            public AudioParameterAsset Parameter;
            [Range(0f, 1f)] public float Value;
        }

        [SerializeField] private ParameterSetting[] _parameters;
        public IReadOnlyList<ParameterSetting> Parameters => _parameters;
    }
}
