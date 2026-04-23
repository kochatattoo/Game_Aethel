using AudioSystem.Components.EquipmentConfigs;
using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Components.Processors;
using Infrastructure.AudioSystem.Components.Sensors;
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

        [Header("Settings")]
        [SerializeField]
        private SurfaceWwiseSwitchResolverConfig _config;
        [SerializeField]
        private BootsMapConfig _bootsConfig;

        private BootsType _bootsType = BootsType.None;

        private IAudioFacade _audioFacade;
        //private IMovementService _movementService;
        //private IEquipmentModel _equipmentModel;
        private CharacterController _controller;

        private FootstepResolverData<AK.Wwise.Switch> _data;
        private IFootstepAudioProcessor _footstepAudioProcessor;
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Текущая нормализованная скорость персонажа (0..1).
        /// Используется резолвером для отсечения звуков при минимальном движении.
        /// </summary>
        public float CurrentSpeed => _agent.velocity.magnitude;

        /// <summary>
        /// Ссылка на контроллер персонажа для проверки состояния приземленности (Grounded).
        /// </summary>
        public CharacterController CharacterController => _controller;

        public BootsType EquipmentType => _bootsType;

        public bool IsGrounded
        {
            get
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
        [Inject]
        public void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
            // _isGrounded = personPhysics.CharacterController;
            // _movementService = movementService;

            // _equipmentModel = equipmentModel;
            // _equipmentModel.EquippedItem.Subscribe(OnEquipmentChanged).AddTo(_disposables);
            // _equipmentModel.UnEquippedItem.Subscribe(OnEquipmentChanged).AddTo(_disposables);

            // UpdateBootsType();
  
            if (!CheckAudioMaker())
                return;

            _data = new FootstepResolverData<AK.Wwise.Switch> (transform, _leftFoot, _rightFoot, _config);
            _footstepAudioProcessor = new FootstepSwitchAudioProcessor( 
                _audioFacade, 
                _audioMaker, 
                _data, 
                this,
                this,
                _bootsConfig);
        }

        /// <summary>
        /// Метод-обработчик, который вызывается напрямую из Animation Events в клипах анимации.
        /// Передает управление внутреннему резолверу.
        /// </summary>
        /// <param name="evt">Событие анимации с параметрами ноги и ассета звука.</param>
        public void PlayFootstep(AnimationEvent evt)
        {
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
                return true;
            } 

            if (_audioMaker == null)
            { 
                Debug.LogError($"AudioMaker not found on {name} or its parents!", this);
                return false;
            }

            return true;
        }

       /* private void OnEquipmentChanged((ItemSlotType slot, IItem item) args)
        {
            if (args.slot == ItemSlotType.Shoes)
            {
                UpdateBootsType();
            }
        }

        private void UpdateBootsType()
        {
            if (_equipmentModel?.EquipmentMap == null)
            {
                _bootsType = BootsType.None;
                return;
            }

            if (_equipmentModel.EquipmentMap.TryGetValue(ItemSlotType.Shoes, out IItem boots) && boots != null)
            {
                var materialType = boots.Parameters?.MaterialType ?? EquipmentMaterialType.None;
                _bootsType = EquipmentMaterialMapper.ToBootsType(materialType);
            }
            else
            {
                _bootsType = BootsType.None;
            }
        }
    */
        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
