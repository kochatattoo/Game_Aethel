using CodeBase.Hero.HeroBehaviour;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
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
        [SerializeField] private Move _move; // необходимо изменить на логику HeroAction
        [SerializeField] private HeroAttack _attack; // необходимо изменить на логику HeroAction
        [SerializeField] private HeroHealth _health; // изменю на загрузку данных в Blackboard - и не придется хранить компонентом (сделаю не монобех)
        [SerializeField] private GameObject _deathFx;

        [Header("Logic")]
        private HeroDeath _death;
        private Blackboard _blackboard; 
        private HeroAction _action;
        private Camera _camera;
        private ClickInputHandler _clickInputHandler;

        public HeroHealth Health {  get { return _health; } }
        public HeroDeath HeroDeath { get { return _death; } }

        public void Construct(IInputService input)
        {
            ConstructControl(input);
            ConstructComponents();
        }

        public void Initialize()
        {
            _death.Initialize();
        }

        private void Update()
        {
           // _action.Update();
        }

        private void ConstructControl(IInputService input)
        {

            _blackboard = new Blackboard();
            _action = new HeroAction(_agent, _blackboard);
            _camera = Camera.main;
            _clickInputHandler = new ClickInputHandler(input, _blackboard, _camera);

            if (_move is HeroPathFollower follower) follower.Construct(input);
            _attack.Construct(input);
        }

        private void ConstructComponents()
        {
            _health.Construct(_animator);
            _death = new HeroDeath(transform, _health, _attack, _move, _animator, _deathFx);
        }
    }
}
