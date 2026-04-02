using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-состояния Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioStateAsset), menuName = ScriptableObjectAudioNames.AudioMenu + "Assets/" + nameof(AudioStateAsset))]
    public class AudioStateAsset : ScriptableObject
    {
        [field: SerializeField, Tooltip("Wwise Switch")]
        public AK.Wwise.State WwiseState { get; private set; }
    }
}
