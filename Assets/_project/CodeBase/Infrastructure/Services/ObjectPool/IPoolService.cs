using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPoolService: IService
    {
        IPool<T> GetPool<T>() where T : Component, IPoolable;
        void AddPool<T>(T prefab, int initialSize) where T : Component, IPoolable;
        void AddPoolToParent<T>(T prefab, Transform parent, int initialSize = 0) where T : Component, IPoolable;
        UniTask AddPoolAsync<T>(T prefab, int initialSize) where T : MonoBehaviour, IPoolable;
        UniTask AddPoolToParentAsync<T>(T prefab, Transform parent, int initialSize = 0)  where T : Component, IPoolable;
        void ShowPools();
        void ClearAllPools();
    }
}