using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public class PoolService : IPoolService
    {
        private readonly Transform _container;
        private readonly Dictionary<Type, IPool> _pools;

        public PoolService()
        {
            _pools = new Dictionary<Type, IPool>();

            GameObject gameObject = new()
            {
                name = "Pool_Container"
            };

            GameObject.Instantiate(gameObject);
            GameObject.DontDestroyOnLoad(gameObject);

            _container = gameObject.transform;
        }

        public void ShowPools()
        {
            foreach (var pool in _pools.Values)
                Debug.Log(pool.ToString());
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
                pool.Clear();

            _pools.Clear();
        }

        public void AddPool<T>(T prefab, int initialSize) 
            where T : Component, IPoolable
        {
            if (PoolsContainKey<T>())
            {
                DebugPoolLog(prefab);
                return;
            }

            _pools.AddPool(prefab, initialSize);
        }

        public void AddPoolToParent<T>( T prefab, Transform parent, int initialSize = 0) 
            where T : Component, IPoolable
        {
            if (PoolsContainKey<T>())
            {
                DebugPoolLog(prefab);
                return;
            }

            _pools.AddPool(prefab, parent, initialSize);
        }

        public void AddPoolToContainer<T>(T prefab, int initialSize = 0)
            where T : Component, IPoolable
        {
            AddPoolToParent(prefab, _container, initialSize);
        }

        public async UniTask AddPoolAsync<T>(T prefab, int initialSize) where T : MonoBehaviour, IPoolable
        {
            if (PoolsContainKey<T>())
            {
                DebugPoolLog(prefab);
                return;
            }

            await _pools.AddPoolAsync(prefab, initialSize);
        }

        public async UniTask AddPoolToParentAsync<T>(T prefab, Transform parent, int initialSize = 0)
            where T : Component, IPoolable
        {
            if (PoolsContainKey<T>())
            {
                DebugPoolLog(prefab);
                return;
            }

            await _pools.AddPoolAsync(prefab, parent, initialSize);
        }

        public async UniTask AddPoolToContainerAsync<T>(T prefab, int initialSize = 0)
          where T : Component, IPoolable
        {
           await AddPoolToParentAsync(prefab, _container, initialSize);
        }

        public IPool<T> GetPool<T>() 
            where T : Component, IPoolable
        {
            if (!_pools.TryGetValue(typeof(T), out var rawPool))
            {
                Debug.LogWarning($"Pool {nameof(T)} doesn't contains in Pools Dictionaru");
                return null;
            }

            return (IPool<T>)rawPool;
        }

        private bool PoolsContainKey<T>()
        {
            return _pools.ContainsKey(typeof(T));
        }

        private void DebugPoolLog<T>(T prefab)
        {
            Debug.Log($"Pool {nameof(prefab)} is existing");
        }
    }
}
