using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using Assets._project.CodeBase.Infrastructure.Services.Input;
using CodeBase.Hero.HeroBehaviour;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using Domain.Character.Core.Sfx;
using Infrastructure.AudioSystem;
using System;
using UnityEngine;
using UnityEngine.AI;
using VFXSystem.Processors;
using VFXSystem.Service;

namespace CodeBase.Hero
{
    /// <summary>
    /// Класс объединяющий в себе все прочие компоненты класса Hero
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class HeroFacade : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private HeroAnimator _animator;
        [SerializeField] private Move _move; // необходимо изменить (или же вовсте оставить один HeroPathFollower) 
        [SerializeField] private HeroAttack _attack; 
        [SerializeField] private HeroHealth _health; // изменю на загрузку данных в Blackboard - и не придется хранить компонентом (сделаю не монобех)
        [SerializeField] private GameObject _deathFx;

        [Header("Logic")]
        private HeroAction _action;
        private HeroDeath _death;
        private Blackboard _blackboard;

        [Header("Sensors")]
        [SerializeField]
        private FootstepMaker _footstep;

        //TODO - Логику обработчика стоит вынести в отдельный сервис или класс обработки всех данных, а передавать уже зависимостью в фасад
        // Так же поступить с blackdoard героя
        private ClickInputHandler _clickInputHandler; 

        private bool _isDie = false;

        public HeroHealth Health {  get { return _health; } }
        public HeroDeath HeroDeath { get { return _death; } }

        public void Construct(IInputHandlerService inputHandlerService, IBlackboardService blackboardService, IAudioFacade audioFacade, IVFXFacade facade)
        {
            ConstructControl(inputHandlerService, blackboardService, facade);
            ConstructComponents();
            ConstructSensors(audioFacade);
        }

        public void Initialize()
        {
            _death.Initialize();
            _death.PlayerDie += HeroDie;
            _action.Initialize();

            _clickInputHandler.Initialize();
            _clickInputHandler.OnProcessed += OnProcessed;
        }

        private void FixedUpdate()
        {
            if (_isDie)
                return;

            _action?.Update();
        }

        private void OnDisable()
        {
            _death.Dispose();
            _clickInputHandler.OnProcessed -= OnProcessed;
        }

        private void OnProcessed()
        {
            //Debug.Log("ResetTree");
            _action.ResetTree();
        }
        private void HeroDie()
        {
            _isDie = true;
        }

        private void ConstructControl(IInputHandlerService inputHandlerService, IBlackboardService blackboardService, IVFXFacade facade)
        {
            if (_move is HeroPathFollower follower) follower.Construct();
            _attack.Construct(facade);

            _blackboard = blackboardService.Blackboard;
            _action = new HeroAction(_blackboard, _move, _attack);
            _clickInputHandler = inputHandlerService.ClickInputHandler;
        }

        private void ConstructComponents()
        {
            _health.Construct(_animator);
            _death = new HeroDeath(transform, _health, _attack, _move, _animator, _deathFx);
        }

        private void ConstructSensors(IAudioFacade audioFacade)
        {
            _footstep.Construct(audioFacade);
        }
    }
}
