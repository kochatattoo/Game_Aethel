using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace CodeBase.Infrastructure.AssetManagement
{
    /// <summary>Owns application asset handles and the instances of the current world.</summary>
    public sealed class AssetProvider : IAsset, IInitializable, IDisposable
    {
        private readonly Dictionary<string, AsyncOperationHandle> _loads = new();
        private readonly List<AsyncOperationHandle<GameObject>> _instances = new();
        private int _generation;
        private bool _disposed;

        public void Initialize() => Addressables.InitializeAsync();
        public Task<T> Load<T>(AssetReference reference) where T : class => LoadInternal<T>(reference.AssetGUID, reference);
        public Task<T> Load<T>(string address) where T : class => LoadInternal<T>(address, address);

        private async Task<T> LoadInternal<T>(string key, object address) where T : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AssetProvider));
            if (!_loads.TryGetValue(key, out var handle))
            {
                handle = Addressables.LoadAssetAsync<T>(address);
                _loads.Add(key, handle);
            }
            try
            {
                await handle.Task;
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new InvalidOperationException("Asset load failed: " + key, handle.OperationException);
            }
            catch
            {
                // Only the waiter that still owns the cached handle releases it.
                if (_loads.TryGetValue(key, out var cached) && cached.Equals(handle))
                {
                    _loads.Remove(key);
                    if (handle.IsValid()) Addressables.Release(handle);
                }
                throw;
            }
            return handle.Result as T ?? throw new InvalidOperationException("Asset type mismatch: " + key);
        }

        public Task<GameObject> Instantiate(string address) => Track(Addressables.InstantiateAsync(address));
        public Task<GameObject> Instantiate(string address, Vector3 at) => Track(Addressables.InstantiateAsync(address, at, Quaternion.identity));
        public Task<GameObject> Instantiate(string address, Transform parent) => Track(Addressables.InstantiateAsync(address, parent));

        private async Task<GameObject> Track(AsyncOperationHandle<GameObject> handle)
        {
            var generation = _generation;
            GameObject instance;
            try { instance = await handle.Task; }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                throw;
            }
            if (_disposed || generation != _generation)
            {
                Addressables.ReleaseInstance(handle);
                throw new OperationCanceledException("World was closed while loading an instance");
            }
            _instances.Add(handle);
            return instance;
        }

        public void CleanUp()
        {
            ++_generation;
            foreach (var handle in _instances)
                if (handle.IsValid()) Addressables.ReleaseInstance(handle);
            _instances.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CleanUp();
            foreach (var handle in _loads.Values)
                if (handle.IsValid()) Addressables.Release(handle);
            _loads.Clear();
        }
    }
}
