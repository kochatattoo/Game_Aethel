using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using UniRx;

namespace Infrastructure.AudioSystem
{
    /// <summary>
    /// Модель состояния аудиосистемы. Хранит и синхронизирует текущие значения 
    /// всех RTPC параметров через реактивные свойства.
    /// </summary>
    public class AudioStateModel
    {
        private readonly Dictionary<AudioParameterAsset, FloatReactiveProperty> _parameters = new();

        /// <summary>
        /// Возвращает реактивное свойство для указанного ассета параметра. 
        /// Если параметр еще не отслеживается, он будет создан со значением по умолчанию.
        /// </summary>
        /// <param name="asset">Ассет параметра Wwise.</param>
        /// <returns>Свойство для чтения и записи значения.</returns>
        public FloatReactiveProperty GetParameterProperty(AudioParameterAsset asset)
        {
            if (!_parameters.ContainsKey(asset))
                _parameters[asset] = new FloatReactiveProperty(asset.DefaultValue);

            return _parameters[asset];
        }

        /// <summary>
        /// Возвращает свойство только для чтения. 
        /// Используется внешними системами (например, UI) для подписки на изменения.
        /// </summary>
        public IReadOnlyReactiveProperty<float> GetProperty(AudioParameterAsset asset) => 
            GetParameterProperty(asset);

        /// <summary>
        /// Устанавливает новое значение для указанного аудио-параметра.
        /// Автоматически уведомляет всех подписчиков об изменении.
        /// </summary>
        /// <param name="asset">Ассет параметра.</param>
        /// <param name="value">Новое числовое значение.</param>
        public void SetValue(AudioParameterAsset asset, float value)
        {
            if (!_parameters.ContainsKey(asset))
                _parameters[asset] = new FloatReactiveProperty(asset.DefaultValue);

            _parameters[asset].Value = value;
        }
    }
}
