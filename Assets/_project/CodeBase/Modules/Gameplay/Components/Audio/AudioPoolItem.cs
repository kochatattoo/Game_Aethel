using CodeBase.Configs;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace CodeBase.Components.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPoolItem : MonoBehaviour, IPoolable<IMemoryPool>
    {
        [SerializeField]
        private AudioSource _source;
        private IMemoryPool _pool;
        private IDisposable _returnTimer;

        public AudioSource Source => _source;

        // Вызывается Zenject при выдаче из пула
        public void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
            _source.enabled = true;
            _source.playOnAwake = false;
        }

        public void Play(AudioConfig config, float duration)
        {
            config.ApplyTo(_source);
            _source.Play();

            _returnTimer = Observable.Timer(TimeSpan.FromSeconds(duration))
                .Subscribe(_ => _pool.Despawn(this));
        }

        public void OnDespawned()
        {
            _returnTimer?.Dispose();
            _source.Stop();
            _source.clip = null;
            transform.SetParent(null); 
            _source.enabled = false;
            _pool = null;
        }

        public class Pool : MonoPoolableMemoryPool<IMemoryPool, AudioPoolItem> { }
    }

}
