using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public class Pool<T> : IPool<T>
        where T : Component, IPoolable
    {
        private readonly T _prefab;
        private readonly Stack<T> _freeObjects = new();
        private  Queue<T> _inUse = new();

        public Pool(T prefab, Transform parent, int initialSize = 0)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                new Pool<T>(prefab, initialSize);

            _prefab = prefab;
            for (int i = 0; i < initialSize; i++)
            {
                var inst = GameObject.Instantiate(prefab, parent);
                inst.gameObject.SetActive(false);
                _freeObjects.Push(inst);
            }
        }

        public Pool(T prefab, int initialSize = 0)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            for (int i = 0; i < initialSize; i++)
            {
                var inst = GameObject.Instantiate(prefab);
                inst.gameObject.SetActive(false);
                _freeObjects.Push(inst);
            }
        }

        public int FreeCount => _freeObjects.Count;

        public void Clear()
        {
            foreach (var inst in _freeObjects)
            {
                GameObject.Destroy(inst.gameObject);
            }
            _freeObjects.Clear();

            foreach( var inst in _inUse)
            {
                inst.OnDespawned();
                GameObject.Destroy(inst.gameObject);
            }
            _inUse.Clear();

        }

        public T Spawn(Transform parent= null, Action<T> initializer = null)
        {
            T gameObject;

            if (_freeObjects.Count > 0)
            {
                gameObject = _freeObjects.Pop();
            }
            else if (_inUse.Count > 0)
            {
                gameObject = _inUse.Dequeue();
            }
            else
            {
                gameObject = parent != null
                    ? GameObject.Instantiate(_prefab, parent)
                    : GameObject.Instantiate(_prefab);
            }

            if (parent != null)
                gameObject.transform.SetParent(parent, false);

            gameObject.gameObject.SetActive(true);

            initializer?.Invoke(gameObject);

            gameObject.OnSpawned();

            _inUse.Enqueue(gameObject);

            return gameObject;
        }

        public void Despawn(T obj)
        {
            obj.OnDespawned();
            obj.gameObject.SetActive(false);

            _inUse = new Queue<T>(_inUse.Where(x => x != obj));

            _freeObjects.Push(obj);
        }

        /// <summary>
        /// Опционально: сбросить все в пул (если вам надо почистить сцену).
        /// </summary>
        public void DespawnAllActive()
        {
            
        }
    }
}
