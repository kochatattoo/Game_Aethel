using UnityEngine.AI;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class AgentMoveToHero : Follow
    {
        private const float MinimalDistance = 2.5f;
        public NavMeshAgent Agent;
        private Transform _heroTransform;

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
            Agent.stoppingDistance = MinimalDistance;
        }
            
        private void Update()
        {
            if (!IsDied)
                Agent.destination = _heroTransform.position;
            else
            { 
                Agent.ResetPath(); 
                gameObject.SetActive(false);
            }
        }
    }
}
