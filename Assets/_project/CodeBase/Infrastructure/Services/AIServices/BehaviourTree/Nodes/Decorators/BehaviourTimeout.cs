using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор-таймаут:
    /// если дочерний узел не завершился в течение timeoutSeconds – отменяет его и возвращает Failure;
    /// если завершился – отдает его статус;
    /// пока ребенок бегает – возвращает Running.
    /// </summary>
    public class BehaviourTimeout : BehaviourNode
    {
        private readonly float _timeoutSeconds;
        private float _startTime;
        private bool _started;

        public BehaviourTimeout(string name, float timeoutSeconds, int priority = 0) : base(name, priority)
        {
            _timeoutSeconds = timeoutSeconds;
        }

        public override Status Process()
        {
            if (Children.Count == 0)
                return Status.Failure;


            if (!_started)
            {
                _started = true;
                _startTime = Time.time;
            }

            if (Time.time - _startTime > _timeoutSeconds)
            {
                Children[0].Reset();
                _started = false;
                return Status.Failure;
            }

            if (Children[0].Process() != Status.Running)
            {
                _started = false;
                return Children[0].Process();
            }

            return Status.Running;

        }

        public override void Reset()
        {
            base.Reset();
            _started = false;
        }
    }
}
