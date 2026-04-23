using Infrastructure.AudioSystem.Parameters.DTO;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Infrastructure.AudioSystem.Parameters
{
    /// <summary>
    /// Высокопроизводительный справочник-резолвер для аудиосистемы.
    /// Преобразует строковые ключи <see cref="AudioKey"/> в нативные объекты Wwise SDK (Event, RTPC, Switch, State).
    /// Использует словари для обеспечения доступа к данным за O(1).
    /// </summary>
    public class WwiseValueMaping
    {
        private readonly WwiseAudioConfig _config;

        private readonly Dictionary<AudioKey, AK.Wwise.Event> _events = new();
        private readonly Dictionary<AudioKey, AK.Wwise.RTPC> _rtpcs = new();
        private readonly Dictionary<AudioKey, AK.Wwise.Switch> _swtch = new();
        private readonly Dictionary<AudioKey, AK.Wwise.State> _stat = new();
        private readonly Dictionary<AudioKey, AK.Wwise.AuxBus> _bus = new();

        /// <summary> Доступ только для чтения ко всем зарегистрированным событиям Wwise. </summary>
        public IReadOnlyDictionary<AudioKey, AK.Wwise.Event> Events => _events;

        /// <summary> Доступ только для чтения ко всем зарегистрированным параметрам (RTPC) Wwise. </summary>
        public IReadOnlyDictionary<AudioKey, AK.Wwise.RTPC> Rtpcs => _rtpcs;

        /// <summary> Доступ только для чтения ко всем зарегистрированным переключателям (Switches) Wwise. </summary>
        public IReadOnlyDictionary<AudioKey, AK.Wwise.Switch> Switches => _swtch;

        /// <summary> Доступ только для чтения ко всем зарегистрированным состояниям (States) Wwise. </summary>
        public IReadOnlyDictionary<AudioKey, AK.Wwise.State> States => _stat;

        /// <summary> Доступ только для чтения ко всем зарегистрированным шинам (AuxBuses) Wwise. </summary>
        public IReadOnlyDictionary<AudioKey, AK.Wwise.AuxBus> AuxBus => _bus;

        /// <summary>
        /// Инициализирует новый экземпляр маппинга, индексируя данные из указанной конфигурации.
        /// </summary>
        /// <param name="config">Исходный ScriptableObject с настройками связей.</param>
        public WwiseValueMaping(WwiseAudioConfig config)
        {
            _config = config;

            foreach (var mapping in _config.Events)
                _events[new AudioKey(mapping.Id)] = mapping.WwiseEvent;

            foreach (var mapping in _config.Rtpcs)
                _rtpcs[new AudioKey(mapping.Id)] = mapping.WwiseRtpc;

            foreach (var mapping in _config.Switches)
                _swtch[new AudioKey(mapping.Id)] = mapping.WwiseSwitch;

            foreach (var mapping in _config.States)
                _stat[new AudioKey(mapping.Id)] = mapping.WwiseState;

            foreach (var mapping in _config.AuxBuses)
                _bus[new AudioKey(mapping.Id)] = mapping.WwiseAuxBus;
        }

        /// <summary>
        /// Пытается найти объект события Wwise по его строковому ключу.
        /// </summary>
        /// <param name="wwiseEvent">Ключ события (например, из констант).</param>
        /// <returns>Объект <see cref="AK.Wwise.Event"/> или null, если ключ не зарегистрирован в конфиге.</returns>
        [CanBeNull]
        public AK.Wwise.Event GetEvent(AudioKey wwiseEvent)
        {
            if(_events.TryGetValue(wwiseEvent, out AK.Wwise.Event ev))
                return ev;

            return null;
        }


        /// <summary>
        /// Пытается найти объект параметра (RTPC) Wwise по его строковому ключу.
        /// </summary>
        /// <param name="parameter">Ключ параметра.</param>
        /// <returns>Объект <see cref="AK.Wwise.RTPC"/> или null, если параметр не найден.</returns>
        [CanBeNull]
        public AK.Wwise.RTPC GetParameter(AudioKey parameter)
        {
            if (_rtpcs.TryGetValue(parameter, out AK.Wwise.RTPC pr))
                return pr;

            return null;
        }

        /// <summary>
        /// Пытается найти объект переключателя (Switch) Wwise по его строковому ключу.
        /// </summary>
        /// <param name="switches">Ключ переключателя.</param>
        /// <returns>Объект <see cref="AK.Wwise.Switch"/> или null.</returns>
        [CanBeNull]
        public AK.Wwise.Switch GetSwitch(AudioKey switches)
        {
            if (_swtch.TryGetValue(switches, out AK.Wwise.Switch sw))
                return sw;

            return null;
        }

        /// <summary>
        /// Пытается найти объект глобального состояния (State) Wwise по его строковому ключу.
        /// </summary>
        /// <param name="state">Ключ состояния.</param>
        /// <returns>Объект <see cref="AK.Wwise.State"/> или null.</returns>
        [CanBeNull]
        public AK.Wwise.State GetState(AudioKey state)
        {
            if (_stat.TryGetValue(state, out AK.Wwise.State st))
                return st;

            return null;
        }

        /// <summary>
        /// Пытается найти объект глобального состояния (AuxBus) Wwise по его строковому ключу.
        /// </summary>
        /// <param name="auxBus">Ключ состояния.</param>
        /// <returns>Объект <see cref="AK.Wwise.AuxBus"/> или null.</returns>
        [CanBeNull]
        public AK.Wwise.AuxBus GetAuxBus(AudioKey auxBus)
        {
            if (_bus.TryGetValue(auxBus, out AK.Wwise.AuxBus bus))
                return bus;

            return null;
        }
    }
}
