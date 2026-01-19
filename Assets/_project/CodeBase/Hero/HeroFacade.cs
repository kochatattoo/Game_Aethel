using CodeBase.Hero.HeroBehaviour;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using System;
using UnityEngine;
using UnityEngine.AI;

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
        [SerializeField] private Move _move; // необходимо изменить на логику HeroAction (или же вовсте оставить один HeroPathFollower) 
        [SerializeField] private HeroAttack _attack; // необходимо изменить на логику HeroAction
        [SerializeField] private HeroHealth _health; // изменю на загрузку данных в Blackboard - и не придется хранить компонентом (сделаю не монобех)
        [SerializeField] private GameObject _deathFx;

        [Header("Logic")]
        private HeroAction _action;
        private HeroDeath _death;
        private Blackboard _blackboard; 
        private Camera _camera;
        private ClickInputHandler _clickInputHandler;

        public HeroHealth Health {  get { return _health; } }
        public HeroDeath HeroDeath { get { return _death; } }

        public void Construct(IInputService input)
        {
            _camera = Camera.main;
            ConstructControl(input);
            ConstructComponents();
        }

        public void Initialize()
        {
            _death.Initialize();
            _action.Initialize();

            _clickInputHandler.Initialize();
            _clickInputHandler.OnProcessed += OnProcessed;
        }

        private void FixedUpdate()
        {
            if (_action != null)
            {
                _action.Update();
            }
        }

        private void OnDisable()
        {
            _death.Dispose();
            _clickInputHandler.OnProcessed -= OnProcessed;
        }

        private void OnProcessed()
        {
            Debug.Log("ResetTree");
            _action.ResetTree();
        }

        private void ConstructControl(IInputService input)
        {
            if (_move is HeroPathFollower follower) follower.Construct();
            _attack.Construct(input);

            _blackboard = new Blackboard();
            _action = new HeroAction(_blackboard, _move);
            _clickInputHandler = new ClickInputHandler(input, _blackboard, _camera);
        }

        private void ConstructComponents()
        {
            _health.Construct(_animator);
            _death = new HeroDeath(transform, _health, _attack, _move, _animator, _deathFx);
        }
    }
}
