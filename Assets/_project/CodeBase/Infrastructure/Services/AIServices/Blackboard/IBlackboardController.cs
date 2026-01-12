using CodeBase.Infrastructure.Services.AIServices.ArbiterLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBase.Infrastructure.Services.AIServices.BlackboardSystem
{
    public interface IBlackboardController : IRegisterExpert
    {
        Blackboard GetBlackboard();
    }
}
