using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public static class PoolExtensions
    {
        public static Dictionary<Type, IPool> AddPool<T>(this Dictionary<Type, IPool> pools, T prefab, int initialSize)
            where T : Component, IPoolable => pools.AddPool(prefab, null, initialSize);

        public static Dictionary<Type, IPool> AddPool<T>(this Dictionary<Type, IPool> pools, T prefab, Transform parent, int initialSize)
            where T : Component, IPoolable
        {
            var pool = new Pool<T>(prefab, parent, initialSize);
            try { pools.Add(typeof(T), pool); }
            catch { pool.Dispose(); throw; }
            return pools;
        }

        public static UniTask<Dictionary<Type, IPool>> AddPoolAsync<T>(this Dictionary<Type, IPool> pools,
            T prefab, int initialSize, bool spreadOverFrames = true) where T : Component, IPoolable =>
            pools.AddPoolAsync(prefab, null, initialSize, spreadOverFrames);

        public static async UniTask<Dictionary<Type, IPool>> AddPoolAsync<T>(this Dictionary<Type, IPool> pools,
            T prefab, Transform parent, int initialSize, bool spreadOverFrames = true) where T : Component, IPoolable
        {
            var pool = new Pool<T>(prefab, parent);
            try
            {
                await pool.PrewarmAsync(initialSize, spreadOverFrames);
                pools.Add(typeof(T), pool);
            }
            catch { pool.Dispose(); throw; }
            return pools;
        }
    }
}
