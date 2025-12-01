using CodeBase.Infrastructure.Services;
using Zenject;

namespace CodeBase.DI
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<NewInputService>()
                .AsSingle()
                .NonLazy();
        }
    }
}