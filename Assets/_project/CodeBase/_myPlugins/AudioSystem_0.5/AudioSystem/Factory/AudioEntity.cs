using Infrastructure.AudioSystem.Parameters.DTO;
using Shared.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Базовая звуковая сущность, управляемая системой пулинга. 
    /// Инкапсулирует логику воспроизведения конкретного Wwise Event на игровом объекте.
    /// </summary>
    [RequireComponent(typeof(AkGameObj))]
    public class AudioEntity : MonoBehaviour, IPoolable<IMemoryPool>
    {
        [SerializeField] 
        private AkGameObj _akGameObj;

        private IMemoryPool _pool;
        private IAudioService _audioService; //Используем сервис, а не фасад по причине иеархии бинда в инсталлере (сервис бинлится раньше пула, а фасад самый последний)
        private uint _playingId;
        private bool _isRegistered = false;

        /// <summary>
        /// Событие, срабатывающее при завершении проигрывания звука или ошибке старта.
        /// Используется фабрикой для возврата объекта в пул.
        /// </summary>
        public EventWrapper<AudioEntity> SoundFinished { get; } = new();

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        /// <summary>
        /// Вызывается автоматически при извлечении объекта из пула.
        /// Регистрирует GameObject в аудио-движке.
        /// </summary>
        public void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise и применяет настройки появления (Fade-in).
        /// </summary>
        /// <param name="eventName">Ключ события для запуска.</param>
        public void Play(AudioKey eventName)
        {
            _playingId = _audioService.PostEvent(eventName, gameObject, OnSoundFinished);

            TryInvalidPlayingId();
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise и применяет настройки появления (Fade-in).
        /// </summary>
        /// <param name="wwiseEvent">Событие для запуска.</param>
        public void Play(AK.Wwise.Event wwiseEvent)
        {
            _playingId = _audioService.PostEvent(wwiseEvent, gameObject, OnSoundFinished);

            TryInvalidPlayingId();
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise с принудительной настройкой переключателя (Switch).
        /// </summary>
        /// <param name="eventName">Ключ события для запуска.</param>
        /// <param name="switchName">Ключ переключателя для запуска.</param>
        public void Play(AudioKey eventName, AudioKey switchName)
        {
            _audioService.SetSwitch(switchName, gameObject);

            Play(eventName);
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise с принудительной настройкой переключателя (Switch).
        /// </summary>
        /// <param name="wwiseEvent">Событие для запуска.</param>
        /// <param name="switchName">Переключатель для запуска.</param>
        public void Play(AK.Wwise.Event wwiseEvent, AK.Wwise.Switch switchName)
        {
            _audioService.SetSwitch(switchName, gameObject);

            Play(wwiseEvent);
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise с предварительной настройкой параметра (Param).
        /// </summary>
        /// <param name="eventName">Ключ события для запуска.</param>
        /// <param name="paramName">Ключ параметра для запуска.</param>
        /// <param name="value">Значение параметра.</param>
        public void Play(AudioKey eventName, AudioKey paramName, float value)
        {
            _audioService.SetRtpc(paramName, value, gameObject);

            Play(eventName);
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise с предварительной настройкой параметра (Param).
        /// </summary>
        /// <param name="wwiseEvent">События для запуска.</param>
        /// <param name="rTPC">Параметр для запуска.</param>
        /// <param name="value">Значение параметра.</param>
        public void Play(AK.Wwise.Event wwiseEvent, AK.Wwise.RTPC rTPC, float value)
        {
            _audioService.SetRtpc(rTPC, value, gameObject);

            Play(wwiseEvent);
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise с полными предварительными настройками (Switch и Param).
        /// </summary>
        /// <param name="eventName">Ключ события для запуска.</param>
        /// <param name="switchName">Ключ переключателя для запуска.</param>
        /// <param name="paramName">Ключ параметра для запуска.</param>
        /// <param name="value">Значение параметра.</param>
        public void Play(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value)
        {
            _audioService.SetRtpc(paramName, value, gameObject);

            Play(eventName, switchName);
        }

        /// <summary>
        /// Запускает воспроизведение указанного события Wwise с полными предварительными настройками (Switch и Param).
        /// </summary>
        /// <param name="wwiseEvent">Событие для запуска.</param>
        /// <param name="switchName">Переключатель для запуска.</param>
        /// <param name="rTPC">Параметр для запуска.</param>
        /// <param name="value">Значение параметра.</param>
        public void Play(AK.Wwise.Event wwiseEvent, AK.Wwise.Switch switchName, AK.Wwise.RTPC rTPC, float value)
        {
            _audioService.SetRtpc(rTPC,value, gameObject);

            Play(wwiseEvent, switchName);
        }

        /// <summary>
        /// Вызывается при возврате объекта в пул. 
        /// Останавливает активные звуки, снимает регистрацию объекта и очищает иерархию (SetParent null).
        /// </summary>
        public void OnDespawned() 
        {
            if (_playingId != _audioService.InvalidPlayingId)
            {
                _audioService.StopPlayingID(_playingId);
                _playingId = _audioService.InvalidPlayingId;
            }

            transform.SetParent(null);
            gameObject.SetActive(false);
            _pool = null;
        }

        /// <summary>
        /// Инициирует процесс возврата объекта в пул через публикацию события завершения.
        /// </summary>
        private void Despawn()
        {
            if (!Application.isPlaying) 
                return;

            SoundFinished.Publish(this); 
        }

        /// <summary>
        /// Внутренний колбэк Wwise. Вызывается в отдельном потоке по завершении звука.
        /// Перенаправляет выполнение в основной поток Unity для безопасного деспауна.
        /// </summary>
        private void OnSoundFinished(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info) => 
            MainThreadDispatcher.Post(_ => Despawn(), null);

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _audioService.UnregisterGameObject(gameObject);
                _isRegistered = false;
            }

            SoundFinished.Dispose();
        }

        /// <summary>
        /// Регистрация объекта в движке Wwise
        /// Для управления, передачи и профилирования звука
        /// (регестрируеми один раз до уничтожения) 
        /// </summary>
        public void RegisterObject()
        {
            //Перенести бы его в отдельное место - возможно сделать публичным и вызывать извне в фабрике
            if (_isRegistered == false)
            {
                _audioService.RegisterGameObject(gameObject);
                _isRegistered = true;
            }
        }

        /// <summary>
        /// Метод для изменения имени объкта, для точного отслеживания в профайлере Wwise
        /// </summary>
        /// <param name="newName"></param>
        public void RenameGameObject(string newName)
        {
            // Можно использовать как внутри класса при Play, так и давать наружу
            // Метод достаточно речурсоемкий - поскольку заново регестрирует объект и обновляет его имя
            // Рассмотреть необходимость его использования
            // Оставляю как доп. функционал - может потребоваться

            gameObject.name = newName;
            _audioService.UpdateObjectName(gameObject);
        }

        private void TryInvalidPlayingId()
        {
            if (_playingId == _audioService.InvalidPlayingId)
            {
                // TODO: Вот тут доработать - нужено событие ошибки воспроизведения

                // Все затухания, рандомизация и громкость отработают на стороне Wwise.
                Debug.LogWarning($"InvalidPlayingId for {_playingId}");
                Despawn();
                return;
            }
        }

        /// <summary>
        /// Стандартная реализация пула для объектов AudioEntity.
        /// </summary>
        public class Pool : MonoMemoryPool<AudioEntity> { }
    }
}
