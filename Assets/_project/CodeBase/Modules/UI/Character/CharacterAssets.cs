using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CodeBase.CharacterIntegration
{
    /// <summary>Ассеты меню живут до закрытия приложения, независимо от очистки уровня.</summary>
    public sealed class CharacterAssets : IDisposable
    {
        public const string Menu = "PetProject/UI/MainMenu";
        public const string Creation = "PetProject/UI/CharacterCreation";
        public const string Attributes = "PetProject/UI/Attributes";
        public const string Classes = "PetProject/Data/HeroClasses";
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

        public async UniTask<T> LoadAsync<T>(string address, CancellationToken token = default) where T : UnityEngine.Object
        {
            if (!_handles.TryGetValue(address, out var handle))
            {
                handle = Addressables.LoadAssetAsync<T>(address);
                _handles.Add(address, handle);
            }
            await handle.Task.AsUniTask().AttachExternalCancellation(token);
            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException($"Не удалось загрузить {address}", handle.OperationException);
            return (T)handle.Result;
        }

        public void Dispose()
        {
            foreach (var handle in _handles.Values) if (handle.IsValid()) Addressables.Release(handle);
            _handles.Clear();
        }
    }
}
