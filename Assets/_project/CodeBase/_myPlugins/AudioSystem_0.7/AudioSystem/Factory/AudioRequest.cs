using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Легковесный конфигуратор (билдер) для настройки и запуска одиночного аудио-события.
    /// Использует типизированные данные движка Wwise. 
    /// Позволяет выстраивать цепочку параметров без аллокаций в куче (Zero-alloc).
    /// </summary>
    public struct AudioRequest
    {
        private AudioEntity _entity;
        private AK.Wwise.Event _event;
        private AK.Wwise.AuxBus _bus;
        private AK.Wwise.Switch _switch;
        private AudioRTPC _rtpc;
        private bool _hasExplicitBus;

        private float _busVolume;
        private AuxSendData _auxData;
        private bool _useBlended;

        public bool IsValid => _entity != null;

        public AudioRequest(AudioEntity entity)
        {
            _entity = entity;
            _event = null;
            _bus = null;
            _switch = null;
            _rtpc = default;
            _hasExplicitBus = false;

            _busVolume = 1f;
            _auxData = default;
            _useBlended = false;
        }

        /// <summary> 
        /// Назначает событие (Wwise Event) для воспроизведения. 
        /// </summary>
        public AudioRequest WithEvent(AK.Wwise.Event ev) { _event = ev; return this; }
        
        /// <summary> 
        /// Назначает вспомогательную шину (AuxBus) для эффектов окружения. 
        /// </summary>
        public AudioRequest WithBus(AK.Wwise.AuxBus bus)
        {
            _bus = bus;
            _hasExplicitBus = true;
            return this;

            // TODO: В будущем может потребоваться что бы передавать вес реверберации
            // Может потребоваться сделать перегрузку и расширение AuxBus в сервисе
        }

        /// <summary>
        /// Назначает AuxPortal для эффектов окружения.
        /// </summary>
        public AudioRequest WithBlendedAuxSends(AK.Wwise.AuxBus busA, float volA, AK.Wwise.AuxBus busB, float volB)
        {
            _auxData = new AuxSendData { AuxBusA = busA, VolumeA = volA, AuxBusB = busB, VolumeB = volB, IsBlended = true };
            _useBlended = true;
            return this;
        }

        /// <summary> 
        /// Устанавливает состояние переключателя (Switch) перед запуском звука.
        /// </summary>
        public AudioRequest WithSwitch(AK.Wwise.Switch sw) { _switch = sw; return this; }

        /// <summary> 
        /// Устанавливает значение RTPC параметра перед запуском звука.
        /// </summary>
        /// <param name="key">Типизированный RTPC параметр.</param>
        /// <param name="val">Числовое значение параметра.</param>
        public AudioRequest WithRtpc(AK.Wwise.RTPC key, float val) { _rtpc = new(key, val); return this; }

        /// <summary> 
        /// Финализирует настройку, применяет все накопленные параметры к AudioEntity и инициирует воспроизведение.
        /// </summary>
        /// <remarks> После вызова Play параметры применяются в строгом порядке: Bus -> Switch -> RTPC -> Event. </remarks>
        public void Play()
        {
            if (_entity == null)
            {
               Debug.LogWarning($"AudioRequest is not valid. AudioEntity: {_entity}");
                return;
            }

            if (_useBlended)
                _entity.InternalPlayBlended(_event, _auxData, _switch, _rtpc);
            else
                _entity.InternalPlay(_event, _bus, _hasExplicitBus, _switch, _rtpc);
        }

        public void Stop()
        {
            if (_entity != null)
                _entity.StopPlaying();
        }
    }
}
