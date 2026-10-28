using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.CharacterIntegration;
using System;

namespace CodeBase.Infrastructure.State
{
    public class LoadProgressState: IState
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly CharacterMenuService _menu;

        public LoadProgressState(IGameStateMachine gameStateMachine, IPersistentProgressService persistentProgressService, ISaveLoadService saveLoadService, CharacterMenuService menu)
        {
            _gameStateMachine = gameStateMachine;
            _progressService = persistentProgressService;
            _saveLoadService = saveLoadService;
            _menu = menu;
        }

        public void Enter()
        {
            try
            {
                if (!_menu.HasPendingNewGame) _progressService.Progress = _saveLoadService.LoadProgress();
                if (!_menu.IsValid(_progressService.Progress))
                {
                    _gameStateMachine.Enter<MainMenuState>();
                    return;
                }
                _menu.RestoreProfile(_progressService.Progress);
                _gameStateMachine.Enter<LoadLevelState, string>(_progressService.Progress.WorldData.PositionOnLevel.Level);
            }
            catch (Exception error) { _menu.LoadingFailed(error); }
        }

        public void Exit() { }

    }
}
