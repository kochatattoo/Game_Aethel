using UnityEngine;

namespace Infrastructure.AudioSystem.WwiseSystem
{
    /// <summary>
    /// Компонент для централизованной инициализации Wwise SDK и управления загрузкой банков по умолчанию.
    /// Агрегирует системные скрипты Wwise для удобного доступа и настройки жизненного цикла движка.
    /// </summary> 
    public class WwiseInitializer : MonoBehaviour
    {
        [SerializeField] 
        private AkInitializer _akInitializer;
        [SerializeField] 
        private AkBank _akBank;

        /// <summary> Ссылка на системный инициализатор Wwise (настройки памяти, потоков и т.д.). </summary>
        public AkInitializer AkInitializer => _akInitializer;

        /// <summary> Основной банк звуков (SoundBank), который должен быть загружен при старте. </summary>
        public AkBank Bank => _akBank;
    }
}
