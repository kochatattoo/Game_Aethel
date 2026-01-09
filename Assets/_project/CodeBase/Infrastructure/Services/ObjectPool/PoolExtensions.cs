using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public static class PoolExtensions
    {
        public static Dictionary<Type, IPool> AddPool<T>(this Dictionary<Type, IPool> dict, T prefab , int initialSize)
        where T : Component, IPoolable
        {
            var pool = new Pool<T>(prefab, initialSize);
            dict.Add(typeof(T), pool);
            return dict;
        }

        public static Dictionary<Type, IPool> AddPool<T>(this Dictionary<Type, IPool> dict, T prefab, Transform parent, int initialSize)
        where T : Component, IPoolable
        {
            var pool = new Pool<T>(prefab, parent, initialSize);
            dict.Add(typeof(T), pool);
            return dict;
        }

        public static Dictionary<Type, IPool> AddPool<T>(this Dictionary<Type, IPool> dict, IPool pool)
        {
            dict.Add(typeof(T), pool);
            return dict;
        }

        public static Dictionary<Type, IPool> AddPool<TPool>(this Dictionary<Type, IPool> dict, Func<IPool> factory)
        where TPool : IPool
        {
            var pool = factory();
            dict.Add(typeof(TPool), pool);
            return dict;
        }

        /// <summary>
        /// Альтернативно — создаём пул по Type через рефлексию.
        /// </summary>
        public static Dictionary<Type, IPool> AddPool(this Dictionary<Type, IPool> dict, Type itemType)
        {
            if (!typeof(IPoolable).IsAssignableFrom(itemType))
                throw new ArgumentException($"Type {itemType} must implement IPoolable");

            // Pool<itemType>
            var poolType = typeof(Pool<>).MakeGenericType(itemType);
            var pool = (IPool)Activator.CreateInstance(poolType);
            dict.Add(itemType, pool);
            return dict;
        }

        public static async UniTask<Dictionary<Type, IPool>> AddPoolAsync<T>(
            this Dictionary<Type, IPool> dict, 
            T prefab,int initialSize, 
            bool spreadOverFrames = true) 
        where T : Component, IPoolable
        {
            var pool = new Pool<T>(prefab, initialSize);
            dict.Add(typeof(T), pool);

            for (int i = 0; i < initialSize; i++)
            {
                var obj = pool.Spawn();   // или: pool.Spawn(parent) если у вас в пуле конструктор без parent
                pool.Despawn(obj);

                if (spreadOverFrames)
                    await UniTask.Yield();
            }

            return dict;
        }

        public static async UniTask<Dictionary<Type, IPool>> AddPoolAsync<T>(
             this Dictionary<Type, IPool> dict,
             T prefab,
             Transform parent,
             int initialSize,
            bool spreadOverFrames = true) 
        where T : Component, IPoolable
        {
            var pool = new Pool<T>(prefab, parent, initialSize);
            dict.Add(typeof(T), pool);

            for (int i = 0; i < initialSize; i++)
            {
                var obj = pool.Spawn(parent);
                pool.Despawn(obj);

                if (spreadOverFrames)
                    await UniTask.Yield();
            }

            return dict;
        }

        public static async UniTask<Dictionary<Type, IPool>> AddPoolAsync<TPool>(
            this Dictionary<Type, IPool> dict,
            Func<IPool> factory,
            int initialSize,
            bool spreadOverFrames = true) 
        where TPool : IPool
        {
            var pool = factory();
            dict.Add(typeof(TPool), pool);

            // если пул реализует IPoolable-спавн (нам нужен доступ к методам Spawn/Despawn)
            if (pool is IPoolableSpawnDespawn psd)
            {
                for (int i = 0; i < initialSize; i++)
                {
                    var obj = psd.Spawn();
                    psd.Despawn(obj);
                    if (spreadOverFrames)
                        await UniTask.Yield();
                }
            }

            return dict;
        }
    }
}
