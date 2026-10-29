using System.Collections.Generic;
using CodeBase.Data;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.Services.PersistentProgress
{
    public interface IProgressRegistry
    {
        IReadOnlyList<ISavedProgressReader> Readers { get; }
        void Register(ISavedProgressReader reader);
        void Unregister(ISavedProgressReader reader);
        void Capture(PlayerProgress progress);
        void Apply(PlayerProgress progress);
        UniTask ApplyAsync(PlayerProgress progress);
        void Clear();
    }

    public interface IAsyncProgressReader : ISavedProgressReader
    {
        UniTask LoadProgressAsync(PlayerProgress progress);
    }
}
