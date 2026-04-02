using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-события Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioEventAsset), menuName = ScriptableObjectAudioNames.AudioMenu + "Assets/" + nameof(AudioEventAsset))]
    public class AudioEventAsset : ScriptableObject
    {
        [field: SerializeField, Tooltip("Wwise Event")]
        public AK.Wwise.Event WwiseEvent { get; private set; }
    }
}
