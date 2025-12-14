namespace CodeBase.Infrastructure.Factory
{
    public interface IServiceFactory
    {
        T CreateService<T>() where T : IService;
    }
}
