using System;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPool
    {
        void Clear();
        void DespawnAllActive();
        int FreeCount { get; }
    }

    public interface IPool<T>: IPool where T : IPoolable
    {
        void Despawn(T obj);
        T Spawn(Transform parent, Action<T> initializer);
    }

}
