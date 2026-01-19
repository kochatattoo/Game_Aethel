using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;

namespace CodeBase.Hero.HeroBehaviour
{
    /// <summary>
    /// Класс овтечающий за выбор поведения героя
    /// </summary>
    public class HeroAction 
    {
        private readonly HeroPathFollower _heroPathFollower;
        private readonly Blackboard _blackboard;
        private readonly BehaviourTree _heroActionTree;
        private readonly HeroBehaviourFactory _heroBehaviourFactory;

        public HeroAction(Blackboard blackboard, Move heroPathFollower)
        {
            if (heroPathFollower is HeroPathFollower follower)
                _heroPathFollower = follower;

            _blackboard = blackboard;
            _heroActionTree = new BehaviourTree("HeroAction");
            _heroBehaviourFactory = new HeroBehaviourFactory(_blackboard, _heroPathFollower);
        }

        public void Initialize()
        {
            IBehaviourNode root = _heroBehaviourFactory.BuildTree();
            _heroActionTree.AddChild(root);
        }

        public void Update()
        {
            Debug.Log("Action Update");
            _heroActionTree.Process();
        }

        public void ResetTree()
        {
            _heroActionTree.Reset();
        }
    }
}
