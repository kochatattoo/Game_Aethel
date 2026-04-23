using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-переключателя Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioSwitchAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioSwitchAsset))]
    public class AudioSwitchAsset : ScriptableObject
    {
        [field: SerializeField, Tooltip("Wwise Switch")]
        public AK.Wwise.Switch WwiseSwitch { get; private set; }
    }
}
