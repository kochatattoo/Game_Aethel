using CodeBase.Infrastructure;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace CodeBase.UI.Services.Factory
{
   public interface IUIFactory: IService
    {
        Task<UnityEngine.GameObject> CreateHud(CodeBase.Logic.IHealth health);
        UnityEngine.Transform Root { get; }
        void CreateOption();
        UniTask CreateOptionAsync();
        void CreateShop();
        UniTask CreateShopAsync();
        UniTask CreateUIRootAsync();
        void WarmUp();
        UniTask WarmUpAsync();
    }
}
