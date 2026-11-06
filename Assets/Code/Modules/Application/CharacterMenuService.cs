using System;
using System.Collections.Generic;
using System.Threading;
using Code.Hero.Data;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.State;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.CharacterIntegration
{
    /// <summary>Application use cases. Presentation, persistence and character state have separate owners.</summary>
    public sealed class CharacterMenuService : IHeroDataProvider, ICharacterMenuActions, IDisposable
    {
        private readonly ICharacterPresentation _presentation;
        private readonly IHeroDataService _hero;
        private readonly IPersistentProgressService _progress;
        private readonly ISaveLoadService _save;
        private readonly ISaveSlotService _slots;
        private readonly ILoadingScreen _curtain;
        private readonly IApplicationHost _host;
        private readonly CharacterSession _session;
        private readonly CharacterClassCatalog _catalog = new();
        private readonly CharacterWorldFlow _worldFlow;
        private readonly CancellationTokenSource _lifetime = new();
        private ICharacterMenuSettings _config;
        private bool _initialized;
        private bool _opening;

        public bool HasPendingNewGame => _worldFlow.HasPendingNewGame;
        public IReadOnlyDictionary<int, HeroClass> HeroClassesReadonly => _catalog.Definitions;

        public CharacterMenuService(ICharacterPresentation presentation, IHeroDataService hero,
            IPersistentProgressService progress, ISaveLoadService save, ISaveSlotService slots,
            IGameStateMachine states, ILoadingScreen curtain, IApplicationHost host, CharacterSession session)
        {
            _presentation = presentation;
            _hero = hero;
            _progress = progress;
            _save = save;
            _slots = slots;
            _curtain = curtain;
            _host = host;
            _session = session;
            _worldFlow = new CharacterWorldFlow(presentation, this, save, slots, states, curtain);
        }

        public UniTask InitializeAsync() => LoadAsync(_lifetime.Token);

        public async UniTask LoadAsync(CancellationToken token)
        {
            if (_initialized) return;
            _config = await _presentation.LoadConfigAsync(token);
            if (_config == null) throw new InvalidOperationException("Не настроен CharacterMenu/Config");
            var json = await _presentation.LoadCatalogAsync(token);
            _catalog.Load(json);
            _session.Configure(_catalog.Definitions, _config.Balance);
            await _presentation.WarmUpAsync(token);
            _slots.InitializeStorage(IsValid, _config.InitialLevel);
            _initialized = true;
        }

        public UniTask ShowMainMenuAsync() => PresentAsync(async () =>
        {
            await _presentation.ShowMenuAsync(this, _worldFlow.WorldReady, IsValid(_save.LoadProgress()),
                _hero.CurrentHero.Value != null, _slots.Notice, _lifetime.Token);
            _curtain.Hide();
        });

        public void StartNewGame() => OpenSlots(true);

        public void OpenSlots(bool newGame = false)
        {
            if (_opening) return;
            _presentation.ShowSlots(this, _slots, _worldFlow.WorldReady, newGame, BeginProfile, LoadSlot,
                _catalog.Definitions);
        }

        private void BeginProfile(string id)
        {
            _slots.PrepareNewProfile(id);
            OpenCreation();
        }

        public void OpenCreation()
        {
            if (!_slots.HasPendingProfile)
            {
                OpenSlots(true);
                return;
            }
            PresentAsync(() => _presentation.ShowCreationAsync(this, _config.Balance, Commit,
                CancelCreation, _lifetime.Token)).Forget(HandleError);
        }

        private void CancelCreation()
        {
            _slots.CancelPendingProfile();
            ShowMainMenuAsync().Forget(HandleError);
        }

        private void Commit(HeroData hero)
        {
            _progress.Progress = PlayerProgressFactory.Create(hero, _config.InitialLevel);
            _hero.SetHero(hero);
            _worldFlow.StartNewGame();
        }

        public void Continue()
        {
            if (_worldFlow.WorldReady)
            {
                Resume();
                return;
            }
            if (_slots.ActiveSlotId != null) LoadSlot(_slots.ActiveSlotId);
        }

        private void LoadSlot(string id)
        {
            try
            {
                _worldFlow.LoadSlot(id);
            }
            catch (Exception error)
            {
                HandleError(error);
            }
        }

        public bool IsValid(PlayerProgress progress) => _session.IsValid(progress);
        public void RestoreProfile(PlayerProgress progress) => _session.RestoreProfile(progress);
        public void BeforeWorldLoad() => _worldFlow.BeforeWorldLoad();
        public void WorldLoaded() => _worldFlow.WorldLoaded();

        public void OpenAttributes()
        {
            if (_opening || _hero.CurrentHero.Value == null) return;
            PresentAsync(() => _presentation.ShowAttributesAsync(_hero, _lifetime.Token)).Forget(HandleError);
        }

        private async UniTask PresentAsync(Func<UniTask> show)
        {
            if (_opening) return;
            _opening = true;
            try
            {
                await show();
            }
            finally
            {
                _opening = false;
            }
        }

        public void Resume() => _presentation.Clear();
        public void LoadingFailed(Exception error) => HandleError(error);

        private void HandleError(Exception error)
        {
            if (error is OperationCanceledException) return;
            _worldFlow.ResetAfterError();
            _slots.CancelPendingProfile();
            Debug.LogException(error);
            _curtain.ShowError("Не удалось загрузить игру. " + error.Message,
                () => ShowMainMenuAsync().Forget(Debug.LogException));
        }

        public void Tick()
        {
            var keys = Keyboard.current;
            if (!_initialized || _opening || keys == null || !_worldFlow.WorldReady) return;
            if (keys.escapeKey.wasPressedThisFrame)
            {
                if (_presentation.HasView) Resume();
                else ShowMainMenuAsync().Forget(HandleError);
            }
            else if (keys.cKey.wasPressedThisFrame && !_presentation.HasView)
            {
                OpenAttributes();
            }
        }

        public void Quit() => _host.Quit();

        public void Dispose()
        {
            _lifetime.Cancel();
            _worldFlow.Dispose();
            _catalog.Dispose();
            _lifetime.Dispose();
        }
    }
}
