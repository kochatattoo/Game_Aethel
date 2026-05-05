using Infrastructure.AudioSystem.Events;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.WwiseSystem
{
    /// <summary>
    /// Компонент для централизованной инициализации Wwise SDK и управления загрузкой банков.
    /// Агрегирует системные скрипты Wwise для удобного доступа и настройки жизненного цикла движка.
    /// </summary> 
    public class WwiseEngineInitializer : MonoBehaviour, IInitializable
    {
        [SerializeField]
        private AkInitializer _akInitializer;
        [SerializeField] 
        private WwiseBankAsset _bankData;

        public void Initialize()
        {
            if (_bankData != null)
            {
                _bankData.Load();
                Debug.Log($"[Wwise] Bank '{_bankData.SoundBank.Name}' loaded via API.");
            }
        }

        private void OnDestroy()
        {
            if (_bankData != null)
                _bankData.Unload();
        }
    }
}
