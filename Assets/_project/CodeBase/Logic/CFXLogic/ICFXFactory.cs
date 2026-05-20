using CodeBase.Infrastructure;
using Cysharp.Threading.Tasks;

namespace Assets._project.CodeBase.Logic.CFXLogic
{
    public interface ICFXFactory: IService
    {
        public void WarmUp();
        public UniTask WarmUpAsync();
        public CFXClickEffect CreateCFXClick();
        public UniTask CreateCFXClickAsync();

    }
}
