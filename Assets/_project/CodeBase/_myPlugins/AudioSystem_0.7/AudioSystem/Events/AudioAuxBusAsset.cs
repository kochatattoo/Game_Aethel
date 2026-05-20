using AK.Wwise;
using Infrastructure.AudioSystem.Abstractions;
using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-шины Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioAuxBusAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioAuxBusAsset))]
    public class AudioAuxBusAsset : ScriptableObject, IAudioAuxBus
    {
        [field: SerializeField, Tooltip("Wwise AuxBus")]
        public AK.Wwise.AuxBus WwiseAuxBus { get; private set; }

        public AuxBus WwiseObject => WwiseAuxBus;

        public bool IsValid() => WwiseAuxBus != null && WwiseAuxBus.IsValid();
    }
}
