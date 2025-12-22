using UnityEngine.AI;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class AgentMoveToHero : Follow
    {
        public NavMeshAgent Agent;
        private Transform _heroTransform;

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
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
