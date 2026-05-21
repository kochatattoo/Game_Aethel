using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;

namespace CodeBase.Hero.HeroBehaviour
{
    // TODO: Вместе с HeroAttackStrategy - можно посмотреть как объединить логику подхода к цели,
    // вынести в отдельную стратегию движения к точке
    internal class HeroInteractStrategy : IStrategy
    {
        private const float COOLDOWN = 1f;
        private const float INTERACT_RANGE = 5f;
        private enum State
        {
            Init,
            MoveTo,
            Interact,
            Cooldown
        }

        private readonly Blackboard _blackboard;
        private readonly HeroPathFollower _follower;

        private State _state;
        private Transform _targetTransform;

        private TargetData _targetData;
        private bool _hasStarted = false;

        public HeroInteractStrategy(Blackboard blackboard, HeroPathFollower pathFollower)
        {
            _blackboard = blackboard;
            _follower = pathFollower;
        }

        public BehaviourNode.Status Process()
        {
            if (!IsTargetValid())
            {
                Reset();
                return BehaviourNode.Status.Failure;
            }

            if (!_hasStarted)
            {
                _hasStarted = true;
                _state = State.Init;
            }

            switch (_state)
            {
                case State.Init:
                    _targetTransform = _targetData.HitObject.transform;
                    _state = State.MoveTo;
                    return BehaviourNode.Status.Running;

                case State.MoveTo:
                    if (IsInRange())
                    {
                        _follower.Stop();
                        _state = State.Interact;
                        return BehaviourNode.Status.Running;
                    }
                    // Логика движения
                    UpdateMoveToTarget();

                    // TODO: Добавить проверку, если _follower.IsStuck, вернуть Failure
                    return BehaviourNode.Status.Running;

                case State.Interact:
                    _follower.transform.LookAt(new Vector3(_targetTransform.position.x, _follower.transform.position.y, _targetTransform.position.z));

                    Debug.Log($"Interact {_targetData.HitObject.name}");

                    _state = State.Cooldown;
                    return BehaviourNode.Status.Running;

                case State.Cooldown:
                    if (Time.time >= COOLDOWN)
                    {
                        _state = State.Init; // Или Success, если дерево должно пересчитаться
                        return BehaviourNode.Status.Success;
                    }
                    return BehaviourNode.Status.Running;
            }

            return BehaviourNode.Status.Failure;
        }

        public void Reset()
        {
            _hasStarted = false;
            _state = State.Init;
            _follower.Stop();
        }

        private bool IsTargetValid()
        {
            BlackboardKey currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
            if (!_blackboard.TryGetValue(currentTarget, out TargetData data))
                return false;

            // Если объект удален или это больше не цель для атаки
            if (data.HitObject == null || !data.HitObject.activeInHierarchy || data.Type != TargetType.Interact)
                return false;

            _targetData = data;
            return true;
        }

        private bool IsInRange()
        {
            Vector3 delta = _targetTransform.position - _follower.transform.position;
            float distXz = new Vector2(delta.x, delta.z).magnitude;
            float heightDiff = Mathf.Abs(delta.y);

            return distXz <= INTERACT_RANGE && heightDiff <= _follower.MaxHeightDifference;
        }

        private void UpdateMoveToTarget()
        {
            Vector3 delta = _targetTransform.position - _follower.transform.position;
            Vector3 horizDir = new Vector3(delta.x, 0, delta.z).normalized;
            Vector3 stopPoint = _targetTransform.position - horizDir * ( 0.9f); // Небольшой допуск (0.9), чтобы не стоять на грани
            _follower.MoveTo(stopPoint);
        }
    }
}