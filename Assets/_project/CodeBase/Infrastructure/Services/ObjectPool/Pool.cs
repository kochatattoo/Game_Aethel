using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public class Pool<T> : IPool<T>
        where T : Component, IPoolable
    {
        private readonly T _prefab;
        private readonly Stack<T> _freeObjects = new();

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

        public T Spawn()
        {
            T obj = _freeObjects.Count>0
                ? _freeObjects.Pop()
                : GameObject.Instantiate(_prefab);
            obj.gameObject.SetActive(true);
            obj.OnSpawned();
            return obj;
        }

        public void Despawn(T obj)
        {
            obj.OnDespawned();
            obj.gameObject.SetActive(false);
            _freeObjects.Push(obj);
        }

        /// <summary>
        /// Опционально: сбросить все в пул (если вам надо почистить сцену).
        /// </summary>
        public void DespawnAllActive()
        {
            // если вы где-то храните список «активных» — можно перебрать и вернуть
        }
    }
}
