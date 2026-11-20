using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;


namespace Code.Character.Data
{
    public interface ICharacterClassProvider
    {
        IReadOnlyDictionary<int, CharacterClass> CharacterClassesReadonly { get; }
        UniTask LoadAsync(CancellationToken token);
    }
}