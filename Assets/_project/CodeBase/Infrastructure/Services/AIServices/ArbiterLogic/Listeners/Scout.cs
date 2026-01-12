using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.ArbiterLogic.Listeners
{
    public class Scout : MonoBehaviour, IExpert
    {
        private Blackboard _blackboard;
        private BlackboardKey _isGetDamageKey;

        private bool _isTakeDamageSensor;

        private void Start()
        {
            //TODO В примере более развернутый ServiceLocator - надо дописать свой
            //_blackboard = ServiceLocator.GetService<IBlackboardController>().GetBlackboard();
           // ServiceLocator.GetService<IBlackboardController>().RegisterExpert(this);
            _isGetDamageKey = _blackboard.GetOrRegisterKey("IsGetDamage");
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                if (_blackboard.TryGetValue(_isGetDamageKey, out bool isGetDamage))
                {
                    _blackboard.SetValue(_isGetDamageKey, !isGetDamage);
                    Debug.Log($"IsTakeDamage: {isGetDamage}");
                }

                if (_isGetDamageKey == null)
                {
                    Debug.Log("Key == null");
                }
                Debug.Log("GetKey");
            }
        }

        public void Execute(Blackboard blackboard)
        {
            blackboard.AddAction(() =>
            {
                if (blackboard.TryGetValue(_isGetDamageKey, out bool isGetDamage))
                {
                    _blackboard.SetValue(_isGetDamageKey, !isGetDamage);
                }
            });
        }

        public int GetInsistence(Blackboard blackboard)
        {
            return _isTakeDamageSensor ? 100 : 0;
        }
    }
}
