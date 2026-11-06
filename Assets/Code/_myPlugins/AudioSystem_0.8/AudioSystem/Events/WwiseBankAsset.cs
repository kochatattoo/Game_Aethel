using AK.Wwise;
using Infrastructure.AudioSystem.Abstractions;
using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио банка Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(WwiseBankAsset), menuName = ScriptableObjectNames.AudioAssets + nameof(WwiseBankAsset))]
    public class WwiseBankAsset : ScriptableObject, IAudioBank
    {
        [field: SerializeField, Tooltip("Wwise Bank")]
        public AK.Wwise.Bank SoundBank { get; private set; }

        public Bank WwiseObject => SoundBank;

        public bool IsValid() => SoundBank != null && SoundBank.IsValid();

        public void Load()
        {
            if (SoundBank.IsValid())
            {
                SoundBank.Load();
            }
        }

        public void Unload()
        {
            if (SoundBank.IsValid())
            {
                SoundBank.Unload();
            }
        }
    }
}
