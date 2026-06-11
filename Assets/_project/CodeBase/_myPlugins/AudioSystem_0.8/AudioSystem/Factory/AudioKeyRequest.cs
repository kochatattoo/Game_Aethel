using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Динамический конфигуратор (билдер) для настройки звука через строковые ключи (AudioKey).
    /// Позволяет управлять воспроизведением, когда данные приходят из внешних источников, 
    /// конфигов или систем, не имеющих прямой ссылки на типы Wwise.
    /// </summary>
    public struct AudioKeyRequest
    {
        private readonly AudioEntity _entity;
        private AudioKey _event;
        private AudioKey _switchKey;
        private AudioKeyRTPC _rtpcKey;
        private AudioKey _bus;

        public bool IsValid => _entity != null;

        public AudioKeyRequest(AudioEntity entity) : this() => _entity = entity;

        /// <summary> 
        /// Назначает аудио-событие по его строковому имени (AudioKey). 
        /// </summary>
        public AudioKeyRequest WithEvent(AudioKey ev) 
        { 
            _event = ev; 
            return this; 
        }

        /// <summary> 
        /// Назначает вспомогательную шину (AuxBus) по её строковому имени. 
        /// </summary>
        public AudioKeyRequest WithBus(AudioKey bus) 
        {
            _bus = bus; 
            return this; 
        }

        /// <summary> 
        /// Устанавливает состояние переключателя (Switch) по строковому ключу. 
        /// </summary>
        public AudioKeyRequest WithSwitch(AudioKey sw) 
        { 
            _switchKey = sw; 
            return this; 
        }

        /// <summary> 
        /// Устанавливает значение RTPC параметра, используя строковый ключ для поиска параметра. 
        /// </summary>
        /// <param name="key">Ключ параметра (имя в Wwise).</param>
        /// <param name="val">Числовое значение.</param>
        public AudioKeyRequest WithRtpc(AudioKey key, float val) 
        { 
            _rtpcKey = new(key, val); 
            return this; 
        }

        /// <summary> 
        /// Финализирует настройку и запускает воспроизведение через строковые идентификаторы.
        /// </summary>
        /// <remarks> 
        /// Метод выполняет проверку строк на пустоту перед отправкой в аудио-движок.
        /// Использование строковых ключей чуть менее производительно, чем типизированных Wwise Types.
        /// </remarks>
        public void Play() 
        {
            if (_entity == null)
            {
                Debug.LogWarning($"AudioRequest is not valid. AudioEntity: {_entity}");
                return;
            }
            _entity.InternalPlayByKey(_event, _bus, _switchKey, _rtpcKey);
        }
    }
}
