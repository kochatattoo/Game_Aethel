using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор-лимитер по времени: даёт дочернему узлу право отработать один раз 
    /// и затем «закрывает» его на заданный интервал, возвращая Running.
    /// </summary>
    public class BehaviourRateLimiter : BehaviourNode
    {
        private readonly float _intervalSeconds;
        private float _nextAllowedTime = 0f;

        public BehaviourRateLimiter(string name, float intervalSeconds, int priority = 0) : base(name, priority)
        {
            _intervalSeconds = intervalSeconds;
        }

        public override Status Process()
        {
            if (Children.Count == 0)
                return Status.Running;

            if (Time.time < _nextAllowedTime)
            {
                return Status.Running;
            }

            if (Children[0].Process() != Status.Running)
            {
                _nextAllowedTime = Time.time + _intervalSeconds;
                Children[0].Reset();
            }

            return Children[0].Process();
        }

        public override void Reset()
        {
            base.Reset();
            _nextAllowedTime = 0f;
        }
    }
}
