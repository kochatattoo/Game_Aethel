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
    }
}
