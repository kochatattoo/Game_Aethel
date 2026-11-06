using UniRx;

namespace Code.Hero.Data
{
    public interface IHeroDataService
    {
        IReadOnlyReactiveProperty<HeroData> CurrentHero { get; }
        void SetHero(HeroData data);
    }
}