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
           // Debug.Log("Attack strategy process");
           // Debug.Log("State " + _state);

            if (!_hasStarted)
            {
                BlackboardKey currentTarget = _blackboard.GetOrRegisterKey("CurrentTarget");
                if (!_blackboard.TryGetValue(currentTarget, out TargetData targetData)
                    || targetData.Type != TargetType.Attack
                    || targetData.HitObject == null)
                    return BehaviourNode.Status.Failure;

                _targetData = targetData;
                _hasStarted = true;

              //  Debug.Log("Object" + _targetData.HitObject);
            }
           
            switch (_state)
            {
                case State.Init:
                    _targetTransform = _targetData.HitObject.transform;
                    _state = State.MoveTo;
                    return BehaviourNode.Status.Running;

                case State.MoveTo:
                    Vector3 delta =  _targetTransform.position - _follower.transform.position;
                    float distXz = new Vector2(delta.x, delta.z).magnitude;

                    float heightDiff = Mathf.Abs(delta.y);
                   // Debug.Log($"AttackRange = {_attack.AttackRange}"+ $"Distance = {distXz:F2}  Height = {heightDiff:F2}");

                    if (distXz > _attack.AttackRange || heightDiff > _follower.MaxHeightDifference)
                    {
                        if (distXz > _attack.AttackRange)
                        {
                            Vector3 horizDir = new Vector3(delta.x, 0, delta.z).normalized;
                            Vector3 stopPoint = _targetTransform.position
                                              - horizDir * _attack.AttackRange;
                            _follower.MoveTo(stopPoint);
                        }
                        else
                        {
                            _follower.MoveTo(_targetTransform.position);
                        }
                        return BehaviourNode.Status.Running;
                    }

                    Debug.Log("Condition true");

                    _follower.Stop();
                    _state = State.Attack;
                    return BehaviourNode.Status.Running;

                case State.Attack:
                    _attack.Attack(_targetTransform);
                    _state = State.Cooldown;
                    _nextAttackTime = Time.time + _attack.AttackCooldown;
                    return BehaviourNode.Status.Running;

                case State.Cooldown:
                    if (Time.time < _nextAttackTime)
                        return BehaviourNode.Status.Running;
                    return BehaviourNode.Status.Success;
            }

            return BehaviourNode.Status.Failure;
        }

        public void Reset()
        {
            _hasStarted = false;
            _state = State.Init;
            _follower.Stop();
        }
    }
}
