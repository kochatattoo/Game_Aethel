using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Events
{
    /// <summary>
    ///  Именованный пресет аудио банка Wwise
    /// </summary>
    [CreateAssetMenu(fileName = nameof(WwiseBankAsset), menuName = ScriptableObjectAudioNames.AudioMenu + "Assets/" + nameof(WwiseBankAsset))]
    public class WwiseBankAsset : ScriptableObject
    {
        [field: SerializeField, Tooltip("Wwise Bank")]
        public AK.Wwise.Bank SoundBank { get; private set; }

        // TODO: Реализовать класс WwiseBankLoader - привязате его к сервису и фасаду, сделать пункт управления загрузкой банков 

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
