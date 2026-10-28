using System;
using UniRx;
namespace Code.Hero.Data
{
    public sealed class HeroDataService : IHeroDataService,IDisposable
    {
        private readonly ReactiveProperty<HeroData> _current=new();
        public IReadOnlyReactiveProperty<HeroData> CurrentHero=>_current;
        public void SetHero(HeroData data){var old=_current.Value;_current.Value=data;if(!ReferenceEquals(old,data))old?.Dispose();}
        public void Dispose(){_current.Value?.Dispose();_current.Dispose();}
    }
}

