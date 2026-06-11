using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components
{
    public enum UpdateMode
    {
        Once,          // Только при старте
        EveryFrame,    // Каждый кадр (для очень быстрых объектов)
        Interval,      // Раз в N секунд (оптимально для большинства)
        Manual         // Только когда вызываем UpdateWwisePositions() извне
    }

    public class MultiPositionAudioComponent : MonoBehaviour
    {
        [SerializeField]
        private bool _playOnStart = true;
        [SerializeField]
        private bool _update = false;

        [Header("Update Settings")]
        [SerializeField] private UpdateMode _updateMode = UpdateMode.Interval;
        [Range(0.02f, 1f)]
        [SerializeField] private float _updateInterval = 0.1f; // Обновление 10 раз в секунду

        [Header("Wwise Settings")]
        [SerializeField]
        private AudioEventAsset _audioEvent;
        [SerializeField]
        private AkMultiPositionType _positionType = AkMultiPositionType.AkMultiPositionType_MultiDirections;

        [Header("Points")]
        [SerializeField]
        private List<Transform> _points = new List<Transform>();

        private IAudioFacade _audioFacade;
        private AkPositionArray _cachedArray;
        private float _timer;
        private int _lastPointsCount;

        [Inject]
        private void Construct(IAudioFacade audioService)
        {
            _audioFacade = audioService;
        }

        private void Start()
        {
            InitializeArray();
            UpdateWwisePositions();

            if (_playOnStart && _audioEvent.WwiseEvent.IsValid())
            {
                _audioFacade.PostEvent(_audioEvent.WwiseEvent, gameObject);
            }
        }

        private void Update()
        {
            if (_update && _points.Count > 0)
            {
                switch (_updateMode)
                {
                    case UpdateMode.EveryFrame:
                        UpdateWwisePositions();
                        break;

                    case UpdateMode.Interval:
                        _timer += Time.deltaTime;
                        if (_timer >= _updateInterval)
                        {
                            _timer = 0;
                            UpdateWwisePositions();
                        }
                        break;

                //TODO: Расширить работу компонента с прочими состояниями
                }
            }
        }

        private void OnDisable()
        {
            if (this == null || gameObject == null)
                return;

            _audioFacade.ClearPositions(gameObject);
            _audioFacade.StopEvent(_audioEvent.WwiseEvent, gameObject);
        }

        public void Play()
        {
            // Метод для внешнего вызова воспроизведения

            if (_audioEvent.WwiseEvent.IsValid())
            {
                _audioFacade.PostEvent(_audioEvent.WwiseEvent, gameObject);
            }
        }

        public void UpdateWwisePositions()
        {
            if (_cachedArray == null || _points.Count == 0)
                return;

            _cachedArray.Reset();

            for (int i = 0; i < _points.Count; i++)
            {
                var t = _points[i];
                if (t != null)
                    _cachedArray.Add(t.position, t.forward, t.up);
            }

            _audioFacade.SetMultiPositionFromCache(gameObject, _cachedArray, _positionType);
        }

        private void InitializeArray()
        {
            if (_points.Count > 0)
            {
                _cachedArray = new AkPositionArray((uint)_points.Count);
            }
        }

        [ContextMenu("Collect Child Points")]
        private void CollectChildren()
        {
            _points.Clear();

            foreach (Transform child in transform)
                _points.Add(child);
        }
    }
}
