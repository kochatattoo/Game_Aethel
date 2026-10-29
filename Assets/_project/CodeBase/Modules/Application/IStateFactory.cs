using CodeBase.Infrastructure.State;

namespace CodeBase.Infrastructure.Factory
{
    public interface IStateFactory
    {
        T CreateState<T>() where T : IExitableState;
    }
}