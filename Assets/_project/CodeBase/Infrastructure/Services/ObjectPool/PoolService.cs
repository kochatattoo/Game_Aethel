using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public class PoolService : IPoolService
    {
        private readonly Dictionary<Type, IPool> _pools;

        public PoolService()
        {
            _pools = new Dictionary<Type, IPool>();
        }

        public void AddPool<T>(T prefab, int initialSize) where T : Component, IPoolable
        {
            if (_pools.ContainsKey(typeof(T)))
            {
                Debug.Log($"Pool {nameof(prefab)} is existing");
                return;
            }
  
            _pools.AddPool<T>(prefab, initialSize);
        }

        public IPool<T> GetPool<T>() where T : Component, IPoolable
        {
            if (!_pools.TryGetValue(typeof(T), out var rawPool))
            {
                Debug.LogWarning($"Pool {nameof(T)} doesn't contains in Pools Dictionaru");
                return null;
            }

            return (IPool<T>)rawPool;
        }
    }
}
