using CodeBase.Infrastructure.Services.AIServices.ArbiterLogic;
using CodeBase.Infrastructure.Services.AIServices.ArbiterLogic.Listeners;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem.Data;
using UnityEngine;

namespace Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem
{
    public class ArbiterBlackboardService: MonoBehaviour, IArbiterBlackboardService
    {
        [SerializeField] private BlackboardData _blackboardData; // Вот тут должен быть InLineEditor
        private readonly Blackboard _blackboard = new Blackboard();
        private readonly IArbiter _arbiter = new Arbiter();

        private void Awake()
        {
            // ServiceLocator.RegisterService<IArbiterBlackboardService>(this);
            _blackboardData.SetValuesOnBlackboard(_blackboard);
            _blackboard.Debug();
        }

        private void Update()
        {
            //Execute all agreed actions from the current iteration
            foreach (var action in _arbiter.BlackboardIterartion(_blackboard))
            {
                action();
            }
        }

        public Blackboard GetBlackboard() => _blackboard;

        public void RegisterExpert(IExpert expert) => _arbiter.RegisterExpert(expert);
        public void DeregisterExpert(IExpert expert) => _arbiter.DeregisterExpert(expert);
    }
}
