using CodeBase.UI.Windows;
using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public class PoolService: IService
    {
        private Dictionary<Type, IPool> _pools;
        
        public PoolService()
        {
            _pools = new Dictionary<Type, IPool>
            {
                [typeof(Pool<WindowBase>)] = new Pool<WindowBase>(),
            };
        }

        public IPool GetPool<T>() where T : IPool
        {
            return _pools[typeof(T)];
        }
    }

    public interface IPool
    {

    }

    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    public class Pool<T>:IPool 
       // where T : Component, IPoolable
    {
        readonly T prefab;
        readonly Stack<T> freeObjects = new();
    }
}
