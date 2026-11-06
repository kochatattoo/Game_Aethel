using AK.Wwise;
using Infrastructure.AudioSystem.Abstractions;
using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио-состояния Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioStateAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(AudioStateAsset))]
    public class AudioStateAsset : ScriptableObject, IAudioState
    {
        [field: SerializeField, Tooltip("Wwise Switch")]
        public AK.Wwise.State WwiseState { get; private set; }

        public State WwiseObject => WwiseState;

        public bool IsValid() => WwiseState != null && WwiseState.IsValid();
    }
}
