using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Logic;
using CodeBase.StaticData;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.State
{
    public class LoadLevelState : IPayloadedState<string>
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly LoadingCurtain _curtain;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private readonly IStaticDataService _staticDataService;

        public LoadLevelState(IGameStateMachine stateMachine, LoadingCurtain curtain, SceneLoader sceneLoader, IGameFactory gameFactory, IStaticDataService staticDataService)
        {
            _stateMachine = stateMachine;
            _curtain = curtain;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
            _staticDataService = staticDataService;
        }

        public void Enter(string sceneName)
        {
            _curtain.Show();
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        public void Exit() { }

        private async void OnLoaded()
        {
            _staticDataService.Load();

           await InitGameWorld();

            _curtain.Hide();
            _stateMachine.Enter<GameLoopState>();
        }

        private async Task InitGameWorld()
        {
            LevelStaticData levelData = LevelStaticData();
            GameObject hero = await InitHero(levelData);
        }

        private async Task<GameObject> InitHero(LevelStaticData levelData) =>
            await _gameFactory.CreateHero(at: levelData.InitialHeroPosition);

        private LevelStaticData LevelStaticData() =>
            _staticDataService.ForLevel(SceneManager.GetActiveScene().name);
    }
}
