using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyAnimator))]
    public class AnimateAlongAgent : MonoBehaviour
    {
        private const float MinimalVelocity = 0.1f;
        public NavMeshAgent Agent;
        public EnemyAnimator Animator;

        private void Update()
        {
            var speed = Agent.velocity.magnitude;
            if (speed > MinimalVelocity && Agent.remainingDistance > Agent.stoppingDistance)
                Animator.Move(speed);
            else
                Animator.StopMoving();
        }
    }
}
