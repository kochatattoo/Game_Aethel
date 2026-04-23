using AudioSystem.Components.EquipmentConfigs;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Factory;
using Shared.Utils.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{
    /// <summary>
    /// Главная база данных аудиосистемы. 
    /// Хранит настройки пулинга, конфигурации Wwise, пресеты воспроизведения и реестр параметров.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioDatabase), menuName = ScriptableObjectNames.AudioMenu + nameof(AudioDatabase))]
    public class AudioDatabase : ScriptableObject
    {
        [Header("Пул объектов")]
        [field: SerializeField, Tooltip("Префаб с компонентом AudioEntity и AkGameObj")]
        public AudioEntity AudioEntityPrefab { get; private set; }

        [field: SerializeField, Tooltip("Размер пула аудио объектов")]
        public int InitialSize { get; private set; } = 10;

        [Header("1.Движок и глобальные настройки")]
        [field: SerializeField, Tooltip("Стандартная громкость при первом запуске"), Range(0, 100)]
        public float DefaultGlobalVolume { get; private set; } = 75f;

        [field: SerializeField, Tooltip("Конфигурация событий и параметров к значениям")]
        public WwiseAudioConfig AudioConfig  { get; private set; }

        [Header("2. Зарегестрированные параметры (RTPC)")]
        [field: SerializeField, Tooltip("Список всех активных параметров, которые AudioService должен отслеживать в модели")]
        private List<AudioParameterAsset> _allParameters = new();
        public IReadOnlyList<AudioParameterAsset> AllParameters => _allParameters;

        [Header("3. Общие (глобальные) события")]
        [field: SerializeField, Tooltip("Часто используемые ивенты (UI, Ambience), к которым нужен быстрый доступ")]
        public List<AudioEventAsset> GlobalEvents { get; private set; } = new();
    }
}
