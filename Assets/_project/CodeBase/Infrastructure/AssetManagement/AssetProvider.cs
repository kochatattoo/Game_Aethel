
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
namespace CodeBase.Infrastructure.AssetManagement
{
    /// <summary>Кеш ассетов принадлежит приложению, экземпляры — текущему уровню.</summary>
    public class AssetProvider : IAsset,IInitializable,IDisposable
    {
        private readonly Dictionary<string,AsyncOperationHandle> _loads=new();
        private readonly List<AsyncOperationHandle<GameObject>> _instances=new();
        private int _generation;
        public void Initialize(){Addressables.InitializeAsync();}
        public Task<T> Load<T>(AssetReference reference) where T:class=>LoadInternal<T>(reference.AssetGUID,reference);
        public Task<T> Load<T>(string address) where T:class=>LoadInternal<T>(address,address);
        private async Task<T> LoadInternal<T>(string key,object address) where T:class
        {
            if(!_loads.TryGetValue(key,out var handle)){handle=Addressables.LoadAssetAsync<T>(address);_loads.Add(key,handle);}
            await handle.Task;
            if(handle.Status!=AsyncOperationStatus.Succeeded)throw new InvalidOperationException("Ошибка загрузки "+key,handle.OperationException);
            return handle.Result as T ?? throw new InvalidOperationException("Неверный тип ассета "+key);
        }
        public Task<GameObject> Instantiate(string address)=>Track(Addressables.InstantiateAsync(address));
        public Task<GameObject> Instantiate(string address,Vector3 at)=>Track(Addressables.InstantiateAsync(address,at,Quaternion.identity));
        public Task<GameObject> Instantiate(string address,Transform parent)=>Track(Addressables.InstantiateAsync(address,parent));
        private async Task<GameObject> Track(AsyncOperationHandle<GameObject> handle)
        {
            var generation=_generation;
            GameObject instance;
            try{instance=await handle.Task;}catch{if(handle.IsValid())Addressables.Release(handle);throw;}
            if(generation!=_generation){Addressables.ReleaseInstance(handle);throw new OperationCanceledException("Уровень уже закрыт");}
            _instances.Add(handle);return instance;
        }
        public void CleanUp()
        {
            _generation++;
            foreach(var handle in _instances)if(handle.IsValid())Addressables.ReleaseInstance(handle);
            _instances.Clear();
        }
        public void Dispose()
        {
            CleanUp();foreach(var handle in _loads.Values)if(handle.IsValid())Addressables.Release(handle);_loads.Clear();
        }
    }
}
