namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPool
    {
        void DespawnAllActive();
        int FreeCount { get; }
    }

    public interface IPool<T>: IPool where T : IPoolable
    {
        void Despawn(T obj);
        T Spawn();
    }

}
