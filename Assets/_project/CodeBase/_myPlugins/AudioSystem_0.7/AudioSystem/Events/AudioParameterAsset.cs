using AK.Wwise;
using Infrastructure.AudioSystem.Abstractions;
using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    /// Контейнер для управления параметрами (RTPC) в Wwise
    /// Позволяет централизованно управлять именами параметров и их значениями по умолчанию.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioParameterAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioParameterAsset))]
    public class AudioParameterAsset : ScriptableObject, IAudioRtpc
    {
        [field: SerializeField]
        public AK.Wwise.RTPC WwiseParameter { get; private set; }

        [field: SerializeField, Range(0, 100)] 
        public float DefaultValue { get; private set; } = 0f;

        public RTPC WwiseObject => WwiseParameter;

        public bool IsValid() => WwiseParameter != null &&  WwiseParameter.IsValid();
    }
}
