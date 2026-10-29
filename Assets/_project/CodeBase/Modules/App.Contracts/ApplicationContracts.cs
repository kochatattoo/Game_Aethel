using System;
using System.Collections.Generic;
using System.Threading;
using Code.Hero.Data;
using CodeBase.Infrastructure.Services.SaveLoad;
using Cysharp.Threading.Tasks;

namespace CodeBase.CharacterIntegration
{
    public interface ICharacterStatsSource { event Action StatsChanged; }

    public interface ICharacterMenuActions
    {
        void StartNewGame();
        void OpenSlots(bool newGame = false);
        void OpenCreation();
        void OpenAttributes();
        void Continue();
        void Resume();
        void Quit();
        UniTask ShowMainMenuAsync();
    }

    public interface ICharacterPresentation : IDisposable
    {
        bool HasView { get; }
        UniTask<string> LoadCatalogAsync(CancellationToken token);
        UniTask WarmUpAsync(CancellationToken token);
        UniTask ShowMenuAsync(ICharacterMenuActions actions, bool worldReady, bool canContinue, bool hasHero, string notice, CancellationToken token);
        void ShowSlots(ICharacterMenuActions actions, ISaveSlotService slots, bool playing, bool newGame,
            Action<string> create, Action<string> load, IReadOnlyDictionary<int, HeroClass> classes);
        UniTask ShowCreationAsync(IHeroDataProvider provider, CharacterBalance balance, Action<HeroData> commit, Action cancel, CancellationToken token);
        UniTask ShowAttributesAsync(IHeroDataService hero, CancellationToken token);
        void Clear();
        void SetToolbar(ICharacterMenuActions actions, bool visible);
    }

    public interface ILoadingScreen
    {
        void Show();
        void Hide();
        void ShowError(string message, Action returnToMenu);
    }

    public interface IApplicationHost { void Quit(); }
    public interface IApplicationWarmup { UniTask PrepareAsync(); }
    public interface IWorldBuilder
    {
        void CleanUp();
        UniTask BuildAsync();
    }
}
