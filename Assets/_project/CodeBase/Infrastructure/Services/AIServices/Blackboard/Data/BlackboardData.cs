using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BlackboardSystem.Data
{
    [CreateAssetMenu(fileName = "New Blackboard Data", menuName = "NPCTools/Blackboard/BlackboardData")]
    public class BlackboardData : ScriptableObject
    {
        public List<BlackboardEntryData> Entries = new();

        public void SetValuesOnBlackboard(Blackboard blackboard)
        {
            foreach (var entry in Entries)
            {
                entry.SetValueOnBlackboard(blackboard);
            }
        }
    }
}
