using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Оптимизированный конфигуратор (билдер) для настройки звука с множественными параметрами.
    /// Позволяет за один вызов применить массивы переключателей и RTPC параметров.
    /// Использует циклы for для минимизации нагрузки на CPU и исключения аллокаций итераторов.
    /// </summary>
    public struct AudioBatchRequest
    {
        private AudioEntity _entity;
        private AK.Wwise.AuxBus _bus;
        private AK.Wwise.Event _event;
        private AK.Wwise.Switch[] _switches;
        private AudioRTPC[] _rtpcs;
        private bool _hasExplicitBus;

        private float _busVolume;
        private AuxSendData _auxData;
        private bool _useBlended;

        public bool IsValid => _entity != null;

        public AudioBatchRequest(AudioEntity entity) : this() => _entity = entity;

        /// <summary> 
        /// Назначает событие (Wwise Event) для воспроизведения. 
        /// </summary>
        public AudioBatchRequest WithEvent(AK.Wwise.Event ev) 
        { 
            _event = ev;
            return this;
        }

        /// <summary> 
        /// Назначает вспомогательную шину (AuxBus) для эффектов окружения. 
        /// </summary>
        public AudioBatchRequest WithBus(AK.Wwise.AuxBus bus)
        {
            _bus = bus;
            _hasExplicitBus = true;
            return this;
        }

        /// <summary>
        /// Назначает AuxPortal для эффектов окружения.
        /// </summary>
        public AudioBatchRequest WithBlendedAuxSends(AK.Wwise.AuxBus busA, float volA, AK.Wwise.AuxBus busB, float volB)
        {
            _auxData = new AuxSendData { AuxBusA = busA, VolumeA = volA, AuxBusB = busB, VolumeB = volB, IsBlended = true };
            _useBlended = true;
            return this;
        }

        /// <summary> 
        /// Передает массив переключателей (Switches). 
        /// Все элементы будут применены последовательно перед запуском события. 
        /// </summary>
        public AudioBatchRequest WithSwitches(AK.Wwise.Switch[] switches)
        {
            _switches = switches;
            return this;
        }

        /// <summary> 
        /// Передает массив RTPC параметров. 
        /// Все значения будут установлены последовательно перед запуском события. 
        /// </summary>
        public AudioBatchRequest WithRTPCs(AudioRTPC[] rtpcs)
        {
            _rtpcs = rtpcs;
            return this;
        }

        /// <summary> 
        /// Финализирует настройку и инициирует воспроизведение. 
        /// Применяет параметры в порядке: Bus -> Batch Switches -> Batch RTPCs -> Event.
        /// </summary>
        /// <remarks> 
        /// Идеально подходит для инициализации звуков с большим количеством предустановок.
        /// </remarks>
        public void Play()
        {
            if (_entity == null)
            {
                Debug.LogWarning($"AudioRequest is not valid. AudioEntity: {_entity}");
                return;
            }

            if (_useBlended)
                _entity.InternalBatchPlayBlended(_event, _auxData, _hasExplicitBus, _switches, _rtpcs);
            else
                _entity.InternalBatchPlay(_event, _bus, _hasExplicitBus, _switches, _rtpcs);
        }
    }
}
