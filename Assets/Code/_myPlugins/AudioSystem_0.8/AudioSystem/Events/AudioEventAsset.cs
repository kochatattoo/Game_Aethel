using Infrastructure.AudioSystem.Abstractions;
using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-события Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioEventAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioEventAsset))]
    public class AudioEventAsset : ScriptableObject, IAudioEvent
    {
        [field: SerializeField, Tooltip("Wwise Event")]
        public AK.Wwise.Event WwiseEvent { get; private set; }

        public bool IsValid() => WwiseEvent != null && WwiseEvent.IsValid();
        AK.Wwise.Event IWwiseWrapper<AK.Wwise.Event>.WwiseObject => WwiseEvent;
    }
}
