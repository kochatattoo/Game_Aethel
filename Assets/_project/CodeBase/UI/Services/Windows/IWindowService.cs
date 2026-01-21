using CodeBase.Infrastructure;
using Cysharp.Threading.Tasks;


namespace CodeBase.UI.Services.Windows
{
    public interface IWindowService: IService
    {
        void Open(WindowId windowId);
        UniTask OpenAsync(WindowId windowId);
    }
}
