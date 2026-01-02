using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPoolService: IService
    {
        IPool<T> GetPool<T>() where T : Component, IPoolable;
        void AddPool<T>(T prefab, int initialSize) where T : Component, IPoolable;
    }
}