using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem.BlackboardExtensions;
using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.Services.AIServices.ArbiterLogic.Listeners
{
    public class Arbiter : IArbiter
    {
        private readonly List<IExpert> _experts = new();

        public void RegisterExpert(IExpert expert)
        {
            Preconditions.CheckNotNull(expert);
            _experts.Add(expert);
        }
        public void DeregisterExpert(IExpert expert)
        {
            Preconditions.CheckNotNull(expert);
            _experts.Remove(expert);
        }

        public List<Action> BlackboardIterartion(Blackboard blackboard)
        {
            IExpert bestExpert = null;
            int highestInsistence = 0;

            foreach (IExpert expert in _experts)
            {
                int insistence = expert.GetInsistence(blackboard);
                if (insistence > highestInsistence)
                {
                    highestInsistence = insistence;
                    bestExpert = expert;
                }
            }

            bestExpert?.Execute(blackboard);

            var actions = blackboard.PassedActions;
            blackboard.ClearActions();

            // Return or execute the actions here

            return actions;
        }
    }
}
