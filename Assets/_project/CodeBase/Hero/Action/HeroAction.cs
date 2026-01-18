using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine.AI;

namespace CodeBase.Hero.HeroBehaviour
{
    /// <summary>
    /// Класс овтечающий за выбор поведения героя
    /// </summary>
    public class HeroAction 
    {
        private readonly NavMeshAgent _agent;
        private readonly Blackboard _blackboard;
        private readonly BehaviourTree _heroActionTree;
        private readonly HeroBehaviourFactory _heroBehaviourFactory;

        public HeroAction(NavMeshAgent agent, Blackboard blackboard)
        {
            _agent = agent;
            _blackboard = blackboard;
            _heroActionTree = new BehaviourTree("HeroAction");
            _heroBehaviourFactory = new HeroBehaviourFactory(_blackboard);
        }

        public void Initialize()
        {
            IBehaviourNode root = _heroBehaviourFactory.BuildTree();
            _heroActionTree.AddChild(root);
        }

        public void Update()
        {
            _heroActionTree.Process();
        }
    }
}
