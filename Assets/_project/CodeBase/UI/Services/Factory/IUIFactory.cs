using CodeBase.Infrastructure;
using System.Threading.Tasks;

namespace CodeBase.UI.Services.Factory
{
   public interface IUIFactory: IService
    {
        void CreateShop();
        Task CreateUIRoot();
    }
}
