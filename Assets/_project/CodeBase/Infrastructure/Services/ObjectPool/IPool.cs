using Cysharp.Threading.Tasks;
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
        T Spawn(Transform parent = null, Action<T> initializer = null);
        T SpawnExpandable(Transform parent = null, Action<T> initializer = null);
        UniTask<T> SpawnAsync(Transform parent = null, Action<T> initializer = null, bool spreadOverFrames = true);
        UniTask<T> SpawnExpandableAsync(Transform parent = null, Action<T> initializer = null, bool spreadOverFrames = true);
        UniTask<T> SpawnExpandableAsync(Action<T> initializer = null, Transform parent = null, bool spreadOverFrames = true);
    }

}
