using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: ждёт первые waitDuration секунд, затем отдаёт управление ребёнку (если он есть),
    /// и после его завершения сбрасывается, чтобы на следующем входе снова ждать.
    /// </summary>
    public class BehaviourWait : BehaviourNode
    {
        private readonly float _waitDuration;
        private float _startTime;
        private bool _waiting;

        public BehaviourWait(string name, float waitDuration, int priority = 0) : base(name, priority)
        {
            _waitDuration = waitDuration;
        }

        public override Status Process()
        {
            if (!_waiting)
            {
                _waiting = true;
                _startTime = Time.time;
            }
            if (Time.time - _startTime < _waitDuration)
            {
                return Status.Running;
            }
            if (Children.Count > 0)
            {
                if (Children[0].Process() != Status.Running)
                {
                    Children[0].Reset();
                    _waiting = false;
                }
                return Children[0].Process();
            }

            _waiting = false;
            return Status.Success;
        }

        public override void Reset()
        {
            base.Reset();
            _waiting = false;
        }
    }
}
