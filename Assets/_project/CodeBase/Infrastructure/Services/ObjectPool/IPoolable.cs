namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
