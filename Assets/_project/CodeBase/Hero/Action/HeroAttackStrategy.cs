using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using UnityEngine;

namespace CodeBase.Hero.HeroBehaviour
{
    public class HeroAttackStrategy : IStrategy
    {
        private enum State { Init, MoveTo, Attack, Cooldown }

        private readonly Blackboard _blackboard;
        private readonly HeroPathFollower _follower;
        private readonly HeroAttack _attack;

        private State _state;
        private float _nextAttackTime;
        private Transform _targetTransform;

        private TargetData _targetData;
        private bool _hasStarted = false;

        public HeroAttackStrategy(Blackboard blackboard, HeroPathFollower pathFollower, HeroAttack attack)
        {
            _blackboard = blackboard;
            _follower = pathFollower;
            _attack = attack;
            _state = State.Init;
        }

        public BehaviourNode.Status Process() //TODO: Разбить на мелкие методы для читабельности
        {
            Debug.Log("Attack strategy process");
            Debug.Log("State " + _state);

            // 1. Валидация цели КАЖДЫЙ кадр
            if (!IsTargetValid())
            {
                Reset();
                return BehaviourNode.Status.Failure;
            }

            // 2. Инициализация (выполняется один раз при старте или смене цели)
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
                        _state = State.Attack;
                        return BehaviourNode.Status.Running;
                    }

                    // Логика движения
                    UpdateMoveToTarget();

                    // TODO: Добавить проверку, если _follower.IsStuck, вернуть Failure
                    return BehaviourNode.Status.Running;

                case State.Attack:
                    // Поворот к цели перед ударом (важно, чтобы не бить воздух)
                    _follower.transform.LookAt(new Vector3(_targetTransform.position.x, _follower.transform.position.y, _targetTransform.position.z));

                    _attack.Attack(_targetTransform);
                    _nextAttackTime = Time.time + _attack.AttackCooldown;
                    _state = State.Cooldown;
                    return BehaviourNode.Status.Running;

                case State.Cooldown:
                    if (Time.time >= _nextAttackTime)
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

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ЧИТАЕМОСТИ ---

        private bool IsTargetValid()
        {
            BlackboardKey currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
            if (!_blackboard.TryGetValue(currentTarget, out TargetData data)) 
                return false;

            // Если объект удален или это больше не цель для атаки
            if (data.HitObject == null || !data.HitObject.activeInHierarchy || data.Type != TargetType.Attack)
                return false;

            _targetData = data;
            return true;
        }

        private bool IsInRange()
        {
            Vector3 delta = _targetTransform.position - _follower.transform.position;
            float distXz = new Vector2(delta.x, delta.z).magnitude;
            float heightDiff = Mathf.Abs(delta.y);

            return distXz <= _attack.AttackRange && heightDiff <= _follower.MaxHeightDifference;
        }

        private void UpdateMoveToTarget()
        {
            Vector3 delta = _targetTransform.position - _follower.transform.position;
            Vector3 horizDir = new Vector3(delta.x, 0, delta.z).normalized;
            Vector3 stopPoint = _targetTransform.position - horizDir * (_attack.AttackRange * 0.9f); // Небольшой допуск (0.9), чтобы не стоять на грани
            _follower.MoveTo(stopPoint);
        }
    }
}
