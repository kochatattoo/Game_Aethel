using CodeBase.Infrastructure.State;
using Zenject;

namespace CodeBase.Infrastructure.Factory
{
    public class StateFactory : IStateFactory
    {
        private readonly DiContainer _container;

        public StateFactory(DiContainer container) =>
            _container = container;

        public T CreateState<T>() where T : IExitableState =>
            _container.Resolve<T>();
    }
}
