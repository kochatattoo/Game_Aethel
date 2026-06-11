using AK.Wwise;
using Infrastructure.AudioSystem.Abstractions;
using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-переключателя Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioSwitchAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioSwitchAsset))]
    public class AudioSwitchAsset : ScriptableObject, IAudioSwitch
    {
        [field: SerializeField, Tooltip("Wwise Switch")]
        public AK.Wwise.Switch WwiseSwitch { get; private set; }

        public Switch WwiseObject => WwiseSwitch;

        public bool IsValid() => WwiseSwitch != null && WwiseSwitch.IsValid();
    }
}
