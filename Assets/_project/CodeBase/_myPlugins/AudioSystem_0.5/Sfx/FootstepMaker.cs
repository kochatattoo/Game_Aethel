using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Components.Processors;
using Infrastructure.AudioSystem.Components.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace Domain.Character.Core.Sfx
{
    /// <summary>
    /// Основной компонент-контроллер для обработки звуков шагов на персонаже.
    /// Собирает данные о костях (ступнях), конфигурацию поверхностей и зависимости движения.
    /// Реализует интерфейс <see cref="IPhysicAudioMaker"/> для предоставления данных о скорости и физике.
    /// </summary>
    public class FootstepMaker : MonoBehaviour, IPhysicAudioMaker
    {
        [Header("Bones References")]
        [SerializeField]
        private Transform _leftFoot;
        [SerializeField]
        private Transform _rightFoot;
        [SerializeField]
        private NavMeshAgent _agent;
        [SerializeField]
        private float _threshold = 0.3f;

        [Header("Settings")]
        [SerializeField]
        private SurfaceWwiseSwitchResolverConfig _config;

        private IAudioFacade _audioFacade;

        private FootstepResolverData<AK.Wwise.Switch> _data;
        private IFootstepAudioProcessor _footstepAudioProcessor;

        /// <summary>
        /// Текущая нормализованная скорость персонажа (0..1).
        /// Используется резолвером для отсечения звуков при минимальном движении.
        /// </summary>
        public float CurrentSpeed => _agent.velocity.magnitude;

        public bool IsGrounded 
        {  get
            {
                if (!_agent.isOnNavMesh) return false;

                // Проверяем, не подпрыгнул ли сам Transform слишком высоко над сеткой
                // agent.nextPosition — это точка на сетке, где "должен" быть агент
                float distanceToNavMesh = Mathf.Abs(transform.position.y - _agent.nextPosition.y);

                return distanceToNavMesh < _threshold;
            } 
        }

    /// <summary>
    /// Внедрение зависимостей и инициализация цепочки резолверов.
    /// Создает контейнер данных <see cref="FootstepResolverData{T}"/> и привязывает конкретный <see cref="FootstepSwitchResolver"/>.
    /// </summary>
    public void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
            _data = new FootstepResolverData<AK.Wwise.Switch> (transform, _leftFoot, _rightFoot, _config);
            _footstepAudioProcessor = new FootstepSwitchAudioProcessor( _audioFacade, _data, this);
        }

        /// <summary>
        /// Метод-обработчик, который вызывается напрямую из Animation Events в клипах анимации.
        /// Передает управление внутреннему резолверу.
        /// </summary>
        /// <param name="evt">Событие анимации с параметрами ноги и ассета звука.</param>
        public void PlayFootstep(AnimationEvent evt)
        {
            _footstepAudioProcessor.PlayOneShot(evt);
        }

        /// <summary>
        /// Отрисовка отладочной информации в редакторе при выделении объекта.
        /// Позволяет видеть лучи каста и текущий определенный материал поверхности.
        /// </summary>
        public void OnDrawGizmosSelected()
        {
           _footstepAudioProcessor?.OnDrawGizmosSelected();
        }
    }
}
