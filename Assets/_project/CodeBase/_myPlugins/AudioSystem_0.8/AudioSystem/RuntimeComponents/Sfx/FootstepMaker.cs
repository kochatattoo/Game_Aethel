using AudioSystem.Components.EquipmentConfigs;
using Cysharp.Threading.Tasks;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Processors;
using Infrastructure.AudioSystem.Components.Sensors;
using Infrastructure.AudioSystem.Factory.GameComponents;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Domain.Character.Core.Sfx
{
    /// <summary>
    /// Основной компонент-контроллер для обработки звуков шагов на персонаже.
    /// Собирает данные о костях (ступнях), конфигурацию поверхностей и зависимости движения.
    /// Реализует интерфейс <see cref="IPhysicAudioMaker"/> для предоставления данных о скорости и физике.
    /// </summary>
    public class FootstepMaker : MonoBehaviour, IPhysicAudioMaker, IEquipmentAudioMaker<BootsType>
    {
        [SerializeField]
        private AudioMaker _audioMaker;

        [Header("Bones References")]
        [SerializeField]
        private Transform _leftFoot;
        [SerializeField]
        private Transform _rightFoot;
        [SerializeField]
        private NavMeshAgent _agent;
        [SerializeField]
        private float _threshold = 0.3f;

        private BootsType _bootsType;

        private IFootstepAudioProcessor _footstepAudioProcessor;
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Текущая нормализованная скорость персонажа (0..1).
        /// Используется резолвером для отсечения звуков при минимальном движении.
        /// </summary>
        public float CurrentSpeed => _agent.velocity.magnitude;

        public BootsType EquipmentType => _bootsType;

        public bool IsGrounded { get; private set; }

 

        /// <summary>
        /// Внедрение зависимостей и инициализация цепочки резолверов.
        /// Создает контейнер данных <see cref="FootstepResolverData{T}"/> и привязывает конкретный <see cref="FootstepSwitchResolver"/>.
        /// </summary>
        [Inject]
        public void Construct(
            IFootstepAudioProcessorFactory processorFactory)
        {
            UpdateBootsType();
            IsGroundedCheck();

            if (!CheckAudioMaker())
                return;

            _footstepAudioProcessor = processorFactory.Create
                (_audioMaker,
                this,
                this,
                transform,
                _leftFoot,
                _rightFoot);
        }

        /// <summary>
        /// Метод-обработчик, который вызывается напрямую из Animation Events в клипах анимации.
        /// Передает управление внутреннему резолверу.
        /// </summary>
        /// <param name="evt">Событие анимации с параметрами ноги и ассета звука.</param>
        public void PlayFootstep(AnimationEvent evt)
        {
            if (!IsGroundedCheck())
                return;

            _footstepAudioProcessor?.PlayOneShot(evt);
        }

        /// <summary>
        /// Отрисовка отладочной информации в редакторе при выделении объекта.
        /// Позволяет видеть лучи каста и текущий определенный материал поверхности.
        /// </summary>
        public void OnDrawGizmosSelected()
        {
           _footstepAudioProcessor?.OnDrawGizmosSelected();
        }

        private bool CheckAudioMaker()
        {
            if (_audioMaker == null)
            {
                _audioMaker = GetComponentInParent<AudioMaker>(); // автоматический поиск
            }

            if (_audioMaker == null)
            {
                Debug.LogError($"AudioMaker not found on {name} or its parents!", this);
                return false;
            }

            return true;
        }

        private bool IsGroundedCheck()
        {
            if (!_agent.isOnNavMesh)
            {
                return IsGrounded = false;
            }

            // Проверяем, не подпрыгнул ли сам Transform слишком высоко над сеткой
            // agent.nextPosition — это точка на сетке, где "должен" быть агент
            float distanceToNavMesh = Mathf.Abs(transform.position.y - _agent.nextPosition.y);

            return IsGrounded = distanceToNavMesh < _threshold;
        }

        private void UpdateBootsType()
        {
            //if (_equipmentModel?.EquipmentMap == null)
            //{
            //    _bootsType = BootsType.None;
            //    return;
            //}

            //if (_equipmentModel.EquipmentMap.TryGetValue(ItemSlotType.Shoes, out IItem boots) && boots != null)
            //{
            //    var materialType = boots.Parameters?.MaterialType ?? EquipmentMaterialType.None;
            //    _bootsType = EquipmentMaterialMapper.ToBootsType(materialType);
            //}
            //else
            //{
            //    _bootsType = BootsType.None;
            //}
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
