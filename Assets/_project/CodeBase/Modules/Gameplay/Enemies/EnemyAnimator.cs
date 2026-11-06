using CodeBase.Logic.Animate;
using System;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class EnemyAnimator : MonoBehaviour, IAnimationStateReader
    {
        private static readonly int Attack_1 = Animator.StringToHash("Attack_1");
        private static readonly int Attack_2 = Animator.StringToHash("Attack_2");
        private static readonly int Attack_3 = Animator.StringToHash("Attack_3");
        private static readonly int Range_Attack = Animator.StringToHash("Range_Attack");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int Win = Animator.StringToHash("Win");

        private readonly int _idleStateHash = Animator.StringToHash("idle");
        private readonly int _attackStateHash1 = Animator.StringToHash("attack01");
        private readonly int _attackStateHash2 = Animator.StringToHash("attack02");
        private readonly int _attackStateHash3 = Animator.StringToHash("attack03");
        private readonly int _rangeAttackStateHash = Animator.StringToHash("rangeAttack");
        private readonly int _walkingStateHash = Animator.StringToHash("walk");
        private readonly int _deathStateHash = Animator.StringToHash("die");

        private readonly int _hitStateHash = Animator.StringToHash("hit");
        private readonly int _winStateHash = Animator.StringToHash("victory");

        private Animator _animator;

        public event Action<AnimatorState> StateEntered;
        public event Action<AnimatorState> StateExited;

        public AnimatorState State {  get; private set; }

        private void Awake() => 
            _animator = GetComponent<Animator>();

        public void PlayHit() =>
            _animator.SetTrigger(Hit);

        public void PlayDeath() =>
            _animator.SetTrigger(Die);

        public void PlayWin() =>
            _animator.SetTrigger(Win);

        public void Move(float speed)
        {
            _animator.SetBool(IsMoving, true);
            _animator.SetFloat(Speed, speed); 
        }

        public void StopMoving() => _animator.SetBool(IsMoving, false);

        public void PlayAttack1() => _animator.SetTrigger(Attack_1);

        public void PlayAttack2() => _animator.SetTrigger(Attack_2);

        public void PlayAttack3() => _animator.SetTrigger(Attack_3);

        public void PlayRangeAttack() => _animator.SetTrigger(Range_Attack);

        public void ResetToIdle() => _animator.Play(_idleStateHash, -1);

        public void EnteredState(int stateHash)
        {
            State = StateFor(stateHash);
            StateEntered?.Invoke(State);
        }

        public void ExitedState(int stateHash) =>
            StateExited?.Invoke(StateFor(stateHash));

        private AnimatorState StateFor(int stateHash)
        {
            AnimatorState state;
            if (stateHash == _idleStateHash)
                state = AnimatorState.Idle;
            else if (CheckAttackHash(stateHash))
                state = AnimatorState.Attack;
            else if (stateHash == _walkingStateHash)
                state = AnimatorState.Walking;
            else if (stateHash == _deathStateHash)
                state = AnimatorState.Died;
            else
                state = AnimatorState.Unknown;

            return state;
        }

        private bool CheckAttackHash(int stateHash)
        {
            return 
                stateHash == _attackStateHash1 
                || stateHash == _attackStateHash2
                || stateHash == _attackStateHash3 
                || stateHash == _rangeAttackStateHash;
        }
    }
}
