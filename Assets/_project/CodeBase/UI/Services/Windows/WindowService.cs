using CodeBase.Infrastructure.Factory;
using CodeBase.UI.Services.Factory;
using Zenject;

namespace CodeBase.UI.Services.Windows
{
    public class WindowService: IWindowService, IInitializable
    {
        private readonly IServiceFactory _serviceFactory;
        private IUIFactory _uiFactory;

        public WindowService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public void Initialize()
        {
            _uiFactory = _serviceFactory.CreateService<IUIFactory>();
        }

        public void Open(WindowId windowId)
        {
            switch (windowId)
            {
                case WindowId.Unknow:
                    break;
                case WindowId.Shop:
                    _uiFactory.CreateShop();
                    break;
                case WindowId.Option:
                    _uiFactory.CreateOption();
                    break;
            }
        }
    }
}
