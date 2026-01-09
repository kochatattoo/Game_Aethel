using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    public interface IPoolable<T>: IPoolable where T: Component, IPoolable
    {
        void SetPool(IPool<T> pool);
    }
}
