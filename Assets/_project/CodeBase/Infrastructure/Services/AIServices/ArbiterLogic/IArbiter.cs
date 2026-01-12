using CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.Services.AIServices.ArbiterLogic
{
    public interface IArbiter : IRegisterExpert
    { 
        List<Action> BlackboardIterartion(Blackboard blackboard);
    }
}
