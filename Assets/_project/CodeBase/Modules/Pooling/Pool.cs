using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public sealed class Pool<T> : IPool<T>, IDisposable where T : Component, IPoolable
    {
        private bool _disposed;
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly bool _ownsParent;
        private readonly Stack<T> _free = new();
        private readonly LinkedList<T> _active = new();
        private readonly Dictionary<T, LinkedListNode<T>> _nodes = new();

        public int FreeCount => _free.Count;
        public Pool(T prefab, int initialSize = 0) : this(prefab, null, initialSize) { }

        public Pool(T prefab, Transform parent, int initialSize = 0)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (initialSize < 0) throw new ArgumentOutOfRangeException(nameof(initialSize));
            _prefab = prefab;
            _ownsParent = parent == null;
            _parent = _ownsParent ? new GameObject(prefab.name + "Pool").transform : parent;
            for (var i = 0; i < initialSize; i++)
            {
                _free.Push(CreateInactive());
            }
        }

        private T CreateInactive()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        internal async UniTask PrewarmAsync(int count, bool spreadOverFrames)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            for (var i = 0; i < count; ++i)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Pool<T>));
                _free.Push(CreateInactive());
                if (spreadOverFrames) await UniTask.Yield();
            }
        }

        public T Spawn(Transform parent = null, Action<T> initializer = null) => Acquire(parent, initializer, false);
        public T SpawnExpandable(Transform parent = null, Action<T> initializer = null) => Acquire(parent, initializer, true);

        private T Acquire(Transform parent, Action<T> initializer, bool expandable)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Pool<T>));
            while (_free.Count > 0 && _free.Peek() == null) _free.Pop();
            if (_free.Count == 0 && !expandable && _active.First != null) Despawn(_active.First.Value);
            var instance = _free.Count > 0 ? _free.Pop() : UnityEngine.Object.Instantiate(_prefab, _parent);
            if (instance is IPoolable<T> typed) typed.SetPool(this);
            instance.transform.SetParent(parent != null ? parent : _parent, false);
            _nodes.Add(instance, _active.AddLast(instance));
            try
            {
                instance.gameObject.SetActive(true);
                initializer?.Invoke(instance);
                instance.OnSpawned();
                return instance;
            }
            catch
            {
                Despawn(instance);
                throw;
            }
        }

        public void Despawn(T instance)
        {
            if (ReferenceEquals(instance, null) || !_nodes.Remove(instance, out var node)) return;
            _active.Remove(node);
            if (instance == null) return;
            instance.OnDespawned();
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_parent, false);
            _free.Push(instance);
        }

        public void DespawnAllActive()
        {
            while (_active.First != null) Despawn(_active.First.Value);
        }

        public void Clear()
        {
            DespawnAllActive();
            while (_free.Count > 0)
            {
                var instance = _free.Pop();
                if (instance != null) Destroy(instance.gameObject);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Clear();
            if (_ownsParent && _parent != null) Destroy(_parent.gameObject);
        }

        private static void Destroy(GameObject instance)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(instance);
            else UnityEngine.Object.DestroyImmediate(instance);
        }

        public async UniTask<T> SpawnAsync(Transform parent = null, Action<T> initializer = null, bool spreadOverFrames = true)
        {
            if (spreadOverFrames) await UniTask.Yield();
            return Spawn(parent, initializer);
        }

        public async UniTask<T> SpawnExpandableAsync(Transform parent = null, Action<T> initializer = null, bool spreadOverFrames = true)
        {
            if (spreadOverFrames) await UniTask.Yield();
            return SpawnExpandable(parent, initializer);
        }

    }
}
