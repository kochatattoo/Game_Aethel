using CodeBase.Infrastructure;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace CodeBase.UI.Services.Factory
{
   public interface IUIFactory: IService
    {
        void CreateOption();
        void CreateShop();
        Task CreateUIRoot();
        void WarmUp();
        UniTask WarmUpAsync();
    }
}
