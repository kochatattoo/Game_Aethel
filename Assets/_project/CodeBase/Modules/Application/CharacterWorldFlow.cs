using System;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.State;

namespace CodeBase.CharacterIntegration
{
    internal sealed class CharacterWorldFlow : IDisposable
    {
        private readonly ICharacterPresentation _presentation;
        private readonly ICharacterMenuActions _actions;
        private readonly ISaveLoadService _save;
        private readonly ISaveSlotService _slots;
        private readonly IGameStateMachine _states;
        private readonly ILoadingScreen _curtain;
        private IDisposable _loadingScope;

        public bool HasPendingNewGame { get; private set; }
        public bool WorldReady { get; private set; }

        public CharacterWorldFlow(ICharacterPresentation presentation, ICharacterMenuActions actions,
            ISaveLoadService save, ISaveSlotService slots, IGameStateMachine states, ILoadingScreen curtain)
        {
            _presentation = presentation;
            _actions = actions;
            _save = save;
            _slots = slots;
            _states = states;
            _curtain = curtain;
        }

        public void StartNewGame()
        {
            HasPendingNewGame = true;
            StartLoading();
            _states.Enter<ReloadGameState>();
        }

        public void LoadSlot(string id)
        {
            if (WorldReady && _slots.ActiveSlotId != id) _save.SaveProgress();
            _slots.SelectSlot(id);
            HasPendingNewGame = false;
            StartLoading();
            _states.Enter<ReloadGameState>();
        }

        public void BeforeWorldLoad() => StartLoading();

        public void WorldLoaded()
        {
            if (HasPendingNewGame)
            {
                _save.SaveProgress();
                HasPendingNewGame = false;
            }
            _loadingScope?.Dispose();
            _loadingScope = null;
            WorldReady = true;
            _presentation.SetToolbar(_actions, true);
        }

        public void ResetAfterError()
        {
            HasPendingNewGame = false;
            _loadingScope?.Dispose();
            _loadingScope = null;
        }

        public void Dispose() => _loadingScope?.Dispose();

        private void StartLoading()
        {
            _presentation.Clear();
            _presentation.SetToolbar(_actions, false);
            WorldReady = false;
            _loadingScope ??= MenuInputScope.Acquire(false);
            _curtain.Show();
        }
    }
}
