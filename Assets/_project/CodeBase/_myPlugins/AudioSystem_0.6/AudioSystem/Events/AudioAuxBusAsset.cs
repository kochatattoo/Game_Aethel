using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-шины Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioAuxBusAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioAuxBusAsset))]
    public class AudioAuxBusAsset : ScriptableObject
    {
        [field: SerializeField, Tooltip("Wwise AuxBus")]
        public AK.Wwise.AuxBus WwiseAuxBus { get; private set; }
    }
}
