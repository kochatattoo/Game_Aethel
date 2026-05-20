using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Occlusions;
using Infrastructure.AudioSystem.Parameters.DTO;
using UniRx;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Базовая звуковая сущность, управляемая системой пулинга Zenject.
    /// Выступает в роли "умной колонки", которая инкапсулирует связь между GameObject и движком Wwise.
    /// Поддерживает гибкую настройку через систему билдеров (AudioRequest) и автоматический возврат в пул.
    /// </summary>
    [RequireComponent(typeof(AkGameObj))]
    public class AudioEntity : MonoBehaviour, IPoolable<IMemoryPool>, IOcclusionEmitter
    {
        [SerializeField] 
        private AkGameObj _akGameObj;
        private AK.Wwise.AuxBus _autoEnvironmentBus; // автоматическое окружение

        private IMemoryPool _pool;
        private IAudioService _audioService;
        private IOcclusionService _occlusionService;
        private uint _playingId;
        private bool _isRegistered = false;

        public Transform EmitterTransform => this.transform;

        [Inject]
        public void Construct(IAudioService audioService, IOcclusionService occlusionService)
        {
            _audioService = audioService;
            _occlusionService = occlusionService;
        }

        /// <summary>
        /// Вызывается автоматически при извлечении объекта из пула.
        /// Регистрирует GameObject в аудио-движке.
        /// </summary>
        public void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;

            RegisterObject();
            _occlusionService?.RegisterEmitter(this);
        }

        /// <summary>
        /// Вызывается при возврате объекта в пул. 
        /// Останавливает активные звуки, снимает регистрацию объекта и очищает иерархию (SetParent null).
        /// </summary>
        public void OnDespawned()
        {
            _occlusionService?.UnregisterEmitter(this);

            if (_playingId != _audioService.InvalidPlayingId)
            {
                _audioService.StopPlayingID(_playingId);
                _playingId = _audioService.InvalidPlayingId;
            }

            _audioService.SetGameObjectAuxSend(gameObject, null);

            _autoEnvironmentBus = null;
            _pool = null;
        }

        /// <summary> 
        /// Полная дерегистрация объекта из аудио-движка при уничтожении GameObject. 
        /// </summary>
        private void OnDestroy()
        {
            _occlusionService?.UnregisterEmitter(this);

            if (_isRegistered)
            {
                _audioService?.UnregisterGameObject(gameObject);
                _isRegistered = false;
            }
        }

        /// <summary>
        /// Метод для установки окружения извне (вызывается фабрикой)
        /// </summary>
        public void SetAutoEnvironmentBus(AK.Wwise.AuxBus bus)
        {
            _autoEnvironmentBus = bus;
        }

        /// <summary> 
        /// Создает конфигуратор для одиночного воспроизведения с использованием типов Wwise. 
        /// </summary>
        public AudioRequest Setup() => new AudioRequest(this);

        /// <summary> 
        /// Создает конфигуратор для воспроизведения со списками переключателей и параметров. 
        /// </summary>
        public AudioBatchRequest SetupBatch() => new AudioBatchRequest(this);

        /// <summary> 
        /// Создает конфигуратор для воспроизведения с использованием строковых ключей (AudioKey).
        /// </summary>
        public AudioKeyRequest SetupByKey() => new AudioKeyRequest(this);

        /// <summary>
        /// Метод для изменения имени объкта, для точного отслеживания в профайлере Wwise
        /// </summary>
        /// <param name="newName"></param>
        public void RenameGameObject(string newName)
        {
            gameObject.name = newName;
            _audioService.UpdateObjectName(gameObject);
        }

        /// <summary> 
        /// Применяет параметры Wwise Types и инициирует PostEvent. 
        /// </summary>
        internal void InternalPlay(AK.Wwise.Event ev, AK.Wwise.AuxBus bus, bool hasExplicitBus, AK.Wwise.Switch sw, AudioRTPC rtpc)
        {
            ApplyBaseParams(bus, hasExplicitBus);

            if (sw != null)
                _audioService.SetSwitch(sw, gameObject);

            if (rtpc.key != null)
                _audioService.SetRtpc(rtpc.key, rtpc.value, gameObject);

            _playingId = _audioService.PostEvent(ev, gameObject, OnSoundFinished);
            TryInvalidPlayingId();
        }

        /// <summary> 
        /// Применяет параметры Wwise Types и инициирует PostEvent. 
        /// </summary>
        internal void InternalPlayBlended(AK.Wwise.Event ev, AuxSendData auxData, AK.Wwise.Switch sw, AudioRTPC rtpc)
        {
            _audioService.SetBlendedAuxSends(gameObject, auxData.AuxBusA, auxData.VolumeA, auxData.AuxBusB, auxData.VolumeB);

            if (sw != null)
                _audioService.SetSwitch(sw, gameObject);

            if (rtpc.key != null)
                _audioService.SetRtpc(rtpc.key, rtpc.value, gameObject);

            _playingId = _audioService.PostEvent(ev, gameObject, OnSoundFinished);
            TryInvalidPlayingId();
        }

        /// <summary> 
        /// Оптимизированное применение массивов параметров через циклы и инициирует PostEvent.
        /// </summary>
        internal void InternalBatchPlay(AK.Wwise.Event ev, AK.Wwise.AuxBus bus, bool hasExplicitBus, AK.Wwise.Switch[] switches, AudioRTPC[] rtpcs)
        {
            ApplyBaseParams(bus, hasExplicitBus);

            if (switches != null)
            {
                for (int i = 0; i < switches.Length; i++)
                {
                    _audioService.SetSwitch(switches[i], gameObject);
                }
            }

            if (rtpcs != null)
            {
                for (int i = 0; i < rtpcs.Length; i++)
                {
                    var r = rtpcs[i];
                    _audioService.SetRtpc(r.key, r.value, gameObject);
                }
            }

            _playingId = _audioService.PostEvent(ev, gameObject, OnSoundFinished);
            TryInvalidPlayingId();
        }

        /// <summary> 
        /// Оптимизированное применение массивов параметров через циклы и инициирует PostEvent.
        /// </summary>
        internal void InternalBatchPlayBlended(AK.Wwise.Event ev, AuxSendData auxData, bool hasExplicitBus, AK.Wwise.Switch[] switches, AudioRTPC[] rtpcs)
        {
            _audioService.SetBlendedAuxSends(gameObject, auxData.AuxBusA, auxData.VolumeA, auxData.AuxBusB, auxData.VolumeB);

            if (switches != null)
            {
                for (int i = 0; i < switches.Length; i++)
                {
                    _audioService.SetSwitch(switches[i], gameObject);
                }
            }

            if (rtpcs != null)
            {
                for (int i = 0; i < rtpcs.Length; i++)
                {
                    var r = rtpcs[i];
                    _audioService.SetRtpc(r.key, r.value, gameObject);
                }
            }

            _playingId = _audioService.PostEvent(ev, gameObject, OnSoundFinished);
            TryInvalidPlayingId();
        }

        /// <summary> 
        /// Применяет параметры через строковые ключи AudioKey и инициирует PostEvent. 
        /// </summary>
        internal void InternalPlayByKey(AudioKey ev, AudioKey bus, AudioKey sw, AudioKeyRTPC rtpc)
        {
            if (string.IsNullOrEmpty(ev.Value))
                return;

            // TODO: Расширить аплаер внутри класса
            if (bus.Value != string.Empty)
                _audioService.SetGameObjectAuxSendKey(gameObject, bus);

            if (sw.Value != string.Empty)
                _audioService.SetSwitch(sw.Value, gameObject);

            if (rtpc.key.Value != string.Empty)
                _audioService.SetRtpc(rtpc.key, rtpc.value, gameObject);

            _playingId = _audioService.PostEvent(ev, gameObject, OnSoundFinished);
            TryInvalidPlayingId();
        }

        /// <summary>
        /// Инициирует процесс возврата объекта в пул через публикацию события завершения.
        /// </summary>
        private void Despawn()
        {
            if (!Application.isPlaying) 
                return;

            _pool?.Despawn(this);
        }

        /// <summary>
        /// Внутренний колбэк Wwise. Вызывается в отдельном потоке по завершении звука.
        /// Перенаправляет выполнение в основной поток Unity для безопасного деспауна.
        /// </summary>
        private void OnSoundFinished(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info) => 
            MainThreadDispatcher.Post(_ => Despawn(), null);

        /// <summary>
        /// Регистрация объекта в движке Wwise
        /// Для управления, передачи и профилирования звука
        /// (регестрируеми один раз до уничтожения) 
        /// </summary>
        private void RegisterObject()
        {
            if (_isRegistered == false)
            {
                _audioService.RegisterGameObject(gameObject);
                _isRegistered = true;
            }
        }

        /// <summary> 
        /// Проверяет валидность PlayingID. В случае ошибки старта немедленно возвращает объект в пул.
        /// </summary>
        private void TryInvalidPlayingId()
        {
            if (_playingId == _audioService.InvalidPlayingId)
            {
                Debug.LogWarning($"InvalidPlayingId for {_playingId}");
                Despawn();
            }
        }

        // TODO
        private void ApplyBaseParams(AK.Wwise.AuxBus explicitBus, bool hasExplicitBus)
        {
            if (hasExplicitBus)
            {
                if (explicitBus != null)
                    _audioService.SetGameObjectAuxSend(gameObject, explicitBus);
                else
                    _audioService.ResetGameObjectAuxSend(gameObject);
            }
            else
            {
                if (_autoEnvironmentBus != null)
                    _audioService.SetGameObjectAuxSend(gameObject, _autoEnvironmentBus);
            }
        }

        public void StopPlaying()
        {
            if (_playingId != _audioService.InvalidPlayingId)
            {
                _audioService.StopPlayingID(_playingId);
                _playingId = _audioService.InvalidPlayingId;
            }
            Despawn(); // возвращаем объект в пул
        }

        /// <summary>
        /// Стандартная реализация пула для объектов AudioEntity.
        /// </summary>
        public class Pool : MonoPoolableMemoryPool<IMemoryPool, AudioEntity> { } 
    }
}
