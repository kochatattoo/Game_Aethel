using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;


namespace Code.Hero.Data
{
    public interface IHeroDataProvider
    {
        IReadOnlyDictionary<int, HeroClass> HeroClassesReadonly { get; }
        UniTask LoadAsync(CancellationToken token);
    }
}