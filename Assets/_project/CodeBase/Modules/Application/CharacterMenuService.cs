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
        private readonly IGameStateMachine _states;
        private readonly ILoadingScreen _curtain;
        private readonly IApplicationHost _host;
        private readonly CharacterSession _session;
        private readonly Dictionary<int, HeroClass> _classes = new();
        private readonly CancellationTokenSource _lifetime = new();
        private CharacterMenuConfig _config;
        private IDisposable _loadingScope;
        private bool _initialized;
        private bool _opening;
        private bool _worldReady;

        public bool HasPendingNewGame { get; private set; }
        public IReadOnlyDictionary<int, HeroClass> HeroClassesReadonly => _classes;

        public CharacterMenuService(ICharacterPresentation presentation, IHeroDataService hero,
            IPersistentProgressService progress, ISaveLoadService save, ISaveSlotService slots,
            IGameStateMachine states, ILoadingScreen curtain, IApplicationHost host, CharacterSession session)
        {
            _presentation = presentation;
            _hero = hero;
            _progress = progress;
            _save = save;
            _slots = slots;
            _states = states;
            _curtain = curtain;
            _host = host;
            _session = session;
        }

        public UniTask InitializeAsync() => LoadAsync(_lifetime.Token);

        public async UniTask LoadAsync(CancellationToken token)
        {
            if (_initialized) return;
            _config = Resources.Load<CharacterMenuConfig>("CharacterMenu/Config");
            if (_config == null) throw new InvalidOperationException("Не настроен CharacterMenu/Config");
            var json = await _presentation.LoadCatalogAsync(token);
            var catalog = JsonUtility.FromJson<HeroClassesRoot>(json);
            if (catalog?.heroClasses == null || catalog.heroClasses.Count == 0)
                throw new InvalidOperationException("Пустой каталог классов");
            foreach (var definition in catalog.heroClasses)
                if (!_classes.TryAdd(definition.id, new HeroClass(definition)))
                    throw new InvalidOperationException("Повторный Id класса");
            _session.Configure(_classes, _config.Balance);
            await _presentation.WarmUpAsync(token);
            _slots.InitializeStorage(IsValid, _config.InitialLevel);
            _initialized = true;
        }

        public UniTask ShowMainMenuAsync() => PresentAsync(async () =>
        {
            await _presentation.ShowMenuAsync(this, _worldReady, IsValid(_save.LoadProgress()),
                _hero.CurrentHero.Value != null, _slots.Notice, _lifetime.Token);
            _curtain.Hide();
        });

        public void StartNewGame() => OpenSlots(true);

        public void OpenSlots(bool newGame = false)
        {
            if (_opening) return;
            _presentation.ShowSlots(this, _slots, _worldReady, newGame, BeginProfile, LoadSlot, _classes);
        }

        private void BeginProfile(string id)
        {
            _slots.PrepareNewProfile(id);
            OpenCreation();
        }

        public void OpenCreation()
        {
            if (!_slots.HasPendingProfile) { OpenSlots(true); return; }
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
            var progress = new PlayerProgress(_config.InitialLevel)
            {
                CharacterSchemaVersion = 1,
                Character = CharacterProfileData.Capture(hero.Hero)
            };
            progress.HeroState.MaxHP = hero.Stats.MaxHealth.Value;
            progress.HeroState.ResetHP();
            progress.HeroStats.Damage = hero.Stats.Damage.Value;
            progress.HeroStats.DamageRadius = 1;
            _progress.Progress = progress;
            _hero.SetHero(hero);
            HasPendingNewGame = true;
            StartLoading();
            _states.Enter<ReloadGameState>();
        }

        public void Continue()
        {
            if (_worldReady) { Resume(); return; }
            if (_slots.ActiveSlotId != null) LoadSlot(_slots.ActiveSlotId);
        }

        private void LoadSlot(string id)
        {
            try
            {
                if (_worldReady && _slots.ActiveSlotId != id) _save.SaveProgress();
                _slots.SelectSlot(id);
                HasPendingNewGame = false;
                StartLoading();
                _states.Enter<ReloadGameState>();
            }
            catch (Exception error) { HandleError(error); }
        }

        public bool IsValid(PlayerProgress progress) => _session.IsValid(progress);
        public void RestoreProfile(PlayerProgress progress) => _session.RestoreProfile(progress);
        public void BeforeWorldLoad() => StartLoading();

        private void StartLoading()
        {
            _presentation.Clear();
            _presentation.SetToolbar(this, false);
            _worldReady = false;
            _loadingScope ??= MenuInputScope.Acquire(false);
            _curtain.Show();
        }

        public void WorldLoaded()
        {
            if (HasPendingNewGame)
            {
                _save.SaveProgress();
                HasPendingNewGame = false;
            }
            _loadingScope?.Dispose();
            _loadingScope = null;
            _worldReady = true;
            _presentation.SetToolbar(this, true);
        }

        public void OpenAttributes()
        {
            if (_opening || _hero.CurrentHero.Value == null) return;
            PresentAsync(() => _presentation.ShowAttributesAsync(_hero, _lifetime.Token)).Forget(HandleError);
        }

        private async UniTask PresentAsync(Func<UniTask> show)
        {
            if (_opening) return;
            _opening = true;
            try { await show(); }
            finally { _opening = false; }
        }

        public void Resume() => _presentation.Clear();
        public void LoadingFailed(Exception error) => HandleError(error);

        private void HandleError(Exception error)
        {
            if (error is OperationCanceledException) return;
            HasPendingNewGame = false;
            _slots.CancelPendingProfile();
            Debug.LogException(error);
            _loadingScope?.Dispose();
            _loadingScope = null;
            _curtain.ShowError("Не удалось загрузить игру. " + error.Message,
                () => ShowMainMenuAsync().Forget(Debug.LogException));
        }

        public void Tick()
        {
            var keys = Keyboard.current;
            if (!_initialized || _opening || keys == null || !_worldReady) return;
            if (keys.escapeKey.wasPressedThisFrame)
            {
                if (_presentation.HasView) Resume();
                else ShowMainMenuAsync().Forget(HandleError);
            }
            else if (keys.cKey.wasPressedThisFrame && !_presentation.HasView) OpenAttributes();
        }

        public void Quit() => _host.Quit();

        public void Dispose()
        {
            _lifetime.Cancel();
            _loadingScope?.Dispose();
            foreach (var definition in _classes.Values) definition.Dispose();
            _classes.Clear();
            _lifetime.Dispose();
        }
    }
}
