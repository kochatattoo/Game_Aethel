using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Nodes.Decorators
{
    /// <summary>
    /// Декоратор: пропускает вызов дочернего узла только если с последнего
    /// завершённого запуска (Success или Failure) прошло >= cooldown секунд.
    /// Иначе сразу возвращает Failure.
    /// </summary>
    public class BehaviourCooldown : BehaviourNode
    {
        private readonly float _cooldown;
        private float _nextAvailableTime;

        public BehaviourCooldown(string name, float cooldownSeconds, int priority = 0)
            : base(name, priority)
        {
            _cooldown = cooldownSeconds;
            _nextAvailableTime = 0f;
        }

        public override Status Process()
        {
            // Убедимся, что есть ровно один дочерний узел
            if (Children.Count == 0)
                return Status.Failure;

            // Если время ещё не пришло — сразу провал
            if (Time.time < _nextAvailableTime)
                return Status.Failure;

            // Иначе вызываем ребёнка
            Status childStatus = Children[0].Process();

            // Если ребёнок завершился — запускаем cooldown
            if (childStatus != Status.Running)
            {
                // следующий доступное время
                _nextAvailableTime = Time.time + _cooldown;
                // сбросим состояние ребёнка на будущее
                Children[0].Reset();
            }

            return childStatus;
        }

        public override void Reset()
        {
            base.Reset();
            _nextAvailableTime = 0f;
        }
    }
}
