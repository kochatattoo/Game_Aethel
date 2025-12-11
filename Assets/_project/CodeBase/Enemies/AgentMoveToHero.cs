using UnityEngine.AI;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class AgentMoveToHero : Follow
    {
        private const float MinimalDistance = 0.8f;
        public NavMeshAgent Agent;
        private Transform _heroTransform;

        public void Construct(Transform heroTransform) =>
            _heroTransform = heroTransform;


        private void Update()
        {
            if (HeroNotReached() && !IsDied)
                Agent.destination = _heroTransform.position;
        }

        private bool HeroNotReached() =>
            Vector3.Distance(Agent.transform.position, _heroTransform.position) >= MinimalDistance;
    }
}
