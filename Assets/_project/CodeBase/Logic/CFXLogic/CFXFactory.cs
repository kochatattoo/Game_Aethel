using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using Assets._project.CodeBase.Infrastructure.Services.Input;
using Cysharp.Threading.Tasks;
using System;

namespace Assets._project.CodeBase.Logic.CFXLogic
{
    public class CFXFactory : ICFXFactory
    {
        private readonly IInputHandlerService _inputHandlerService;
        private readonly IBlackboardService _blackboardService;

        public CFXFactory(IInputHandlerService inputHandlerService, 
                          IBlackboardService blackboardService)
        {
            _inputHandlerService = inputHandlerService;
            _blackboardService = blackboardService;
        }

        public CFXClickEffect CreateCFXClick()
        {
            throw new NotImplementedException();
        }

        public UniTask CreateCFXClickAsync()
        {
            throw new NotImplementedException();
        }

        public void WarmUp()
        {
            throw new NotImplementedException();
        }

        public UniTask WarmUpAsync()
        {
            throw new NotImplementedException();
        }
    }
}
