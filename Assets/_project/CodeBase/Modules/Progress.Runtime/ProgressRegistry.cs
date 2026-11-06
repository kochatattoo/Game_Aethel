using System;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public sealed class ProgressRegistry : IProgressRegistry
    {
        private readonly List<ISavedProgressReader> _readers = new();
        public IReadOnlyList<ISavedProgressReader> Readers => _readers;

        public void Register(ISavedProgressReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (!_readers.Contains(reader)) _readers.Add(reader);
        }

        public void Unregister(ISavedProgressReader reader) => _readers.Remove(reader);
        public void Clear() => _readers.Clear();

        public void Capture(PlayerProgress progress)
        {
            foreach (var reader in _readers.ToArray())
                if (reader is ISavedProgress writer) writer.UpdateProgress(progress);
        }

        public void Apply(PlayerProgress progress)
        {
            foreach (var reader in _readers.ToArray()) reader.LoadProgress(progress);
        }

        public async UniTask ApplyAsync(PlayerProgress progress)
        {
            foreach (var reader in _readers.ToArray())
            {
                if (reader is IAsyncProgressReader asynchronous) await asynchronous.LoadProgressAsync(progress);
                else reader.LoadProgress(progress);
            }
        }
    }
}
