using Cysharp.Threading.Tasks;
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
        private readonly Transform _defaultParent;
        private readonly Stack<T> _freeObjects = new();
        private  Queue<T> _inUse = new();

        public Pool(T prefab, int initialSize = 0)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _defaultParent = GameObject.Instantiate(new GameObject() { name = prefab.name }).transform;
            _prefab = prefab;

            for (int i = 0; i < initialSize; i++)
            {
                var inst = GameObject.Instantiate(prefab);
                inst.gameObject.SetActive(false);
                _freeObjects.Push(inst);
            }
        }

        public Pool(T prefab, Transform parent, int initialSize = 0)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                new Pool<T>(prefab, initialSize);

            _defaultParent = parent;
            _prefab = prefab;

            for (int i = 0; i < initialSize; i++)
            {
                var inst = GameObject.Instantiate(prefab, parent);
                inst.gameObject.SetActive(false);
                _freeObjects.Push(inst);
            }
        }

        public int FreeCount => _freeObjects.Count;

        public void Clear()
        {
            foreach (var inst in _freeObjects)
            {
                if(inst != null && inst.gameObject != null)
                GameObject.Destroy(inst.gameObject);
            }
            _freeObjects.Clear();

            foreach( var inst in _inUse)
            {
                if (inst != null && inst.gameObject != null)
                { 
                    inst.OnDespawned();
                    GameObject.Destroy(inst.gameObject);
                }
            }
            _inUse.Clear();
        }

        public T Spawn(Transform parent= null, Action<T> initializer = null)
        {
            T instance;

            if (_freeObjects.Count > 0)
            {
                instance = _freeObjects.Pop();
            }
            else if (_inUse.Count > 0)
            {
                instance = _inUse.Dequeue();
                instance.OnDespawned();
                instance.gameObject.SetActive(false);
            }
            else
            {
                instance = CreateInstance(parent);
            }

            if (instance is IPoolable<T>poolable) 
               poolable.SetPool(this);

            SetParent(parent, instance);
            ActivateInstance(initializer, instance);

            return instance;
        }

        public T SpawnExpandable(Transform parent = null, Action<T> initializer = null)
        {
            if (_freeObjects.Count > 0)
                return Spawn(parent, initializer);

            var instance = CreateInstance(parent);

            if (instance is IPoolable<T> poolable)
                poolable.SetPool(this);

            SetParent(parent, instance);
            ActivateInstance(initializer, instance);

            return instance;
        }

        public async UniTask<T> SpawnAsync(Transform parent = null, Action<T> initializer = null, bool spreadOverFrames = true)
        {
            if (spreadOverFrames)
                await UniTask.Yield();

            return Spawn(parent, initializer);
        }

        public async UniTask<T> SpawnExpandableAsync(Transform parent = null, Action<T> initializer = null, bool spreadOverFrames = true)
        {
            if (spreadOverFrames)
                await UniTask.Yield();

            return SpawnExpandable(parent, initializer);
        }

        public void Despawn(T obj)
        {
            obj.OnDespawned();
            obj.gameObject.SetActive(false);
            obj.gameObject.transform.SetParent(_defaultParent, false);

            _inUse = new Queue<T>(_inUse.Where(x => x != obj));

            _freeObjects.Push(obj);
        }

        public void DespawnAllActive()
        {
            foreach (var inst in _inUse)
            {
                inst.OnDespawned();
            }
        }

        private void SetParent(Transform parent, T gameObject)
        {
            Transform p = parent != null ? parent : _defaultParent;
            if (p != null)
                gameObject.transform.SetParent(p, false);
        }

        private void ActivateInstance(Action<T> initializer, T instance)
        {
            instance.gameObject.SetActive(true);
            initializer?.Invoke(instance);
            instance.OnSpawned();
            _inUse.Enqueue(instance);
        }

        // Вспомогательный инстанциатор
        private T CreateInstance(Transform parent)
        {
            return parent != null
                ? GameObject.Instantiate(_prefab, parent, false)
                : GameObject.Instantiate(_prefab);
        }
    }
}
