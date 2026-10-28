using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.Utils;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using CodeBase.CharacterIntegration;


namespace CodeBase.Infrastructure.State
{
    public class LoadLevelState : IPayloadedState<string>
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly LoadingCurtain _curtain;
        private readonly SceneLoader _sceneLoader;
        private readonly IGameFactory _gameFactory;
        private readonly IPersistentProgressService _progressService;
        private readonly IStaticDataService _staticDataService;
        private readonly CharacterMenuService _menu;

        public LoadLevelState(IGameStateMachine stateMachine,
            LoadingCurtain curtain,
            SceneLoader sceneLoader,
            IGameFactory gameFactory,
            IPersistentProgressService progressService,
            IStaticDataService staticDataService, CharacterMenuService menu)
        {
            _stateMachine = stateMachine;
            _curtain = curtain;
            _sceneLoader = sceneLoader;
            _gameFactory = gameFactory;
            _staticDataService = staticDataService;
            _progressService = progressService;
            _menu = menu;
        }

        public void Enter(string sceneName)
        {
            _menu.BeforeWorldLoad();
            _curtain.Show();
            _gameFactory.CleanUp();
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        public void Exit() { }

        private async void OnLoaded()
        {
            try
            {
                await InitGameWorld();
                InformProgressReaders();
                _menu.WorldLoaded();
                _curtain.Hide();
                _stateMachine.Enter<GameLoopState>();
            }
            catch (Exception error) { _menu.LoadingFailed(error); }
        }

        private void InformProgressReaders()
        {
            foreach (ISavedProgressReader progressReader in _gameFactory.ProgressReaders)
            {
                progressReader.LoadProgress(_progressService.Progress);
            }
        }

        private async Task InitGameWorld()
        {
            LevelStaticData levelData = LevelStaticData();

            await InitSpawners(levelData);
            await InitLoots();
            await InitSaveTrigger(levelData);
            await InitTransferToPoints(levelData);

            HeroFacade hero = await InitHero(levelData);
            hero.Health.BindStats(_menu);
            await InitHud(hero);

            Camera.main.GetComponent<CameraFollow>().Construct(hero.transform);
            Camera.main.GetComponent<CameraOcclusionFade>().Construct(hero.transform);
        }

        private async Task<HeroFacade> InitHero(LevelStaticData levelData) =>
            await _gameFactory.CreateHero(at: levelData.InitialHeroPosition);

        private async Task InitHud(HeroFacade hero)
        {
            GameObject hud = await _gameFactory.CreateHud();

            hud.GetComponentInChildren<ActorUI>()
                .Construct(hero.Health);
        }

        private async Task InitTransferToPoints(LevelStaticData levelData)
        {
            if (!string.IsNullOrWhiteSpace(levelData.LevelTransferData.LevelTo))
                await _gameFactory.CreateTransferToPoint(levelData.LevelTransferData);
        }

        private async Task InitLoots()
        {
            foreach (var id in _progressService.Progress.WorldData.LootData.LootsOnGround.Dict)
            {
                if (id.Key.StartsWith(SceneManager.GetActiveScene().name + "_", StringComparison.Ordinal))
                    await _gameFactory.CreateLootFromPool(id.Key);
            }
        }

        private async Task InitSpawners(LevelStaticData levelData)
        {
            foreach (EnemySpawnerData spawnerData in levelData.EnemySpawners)
            {
                await _gameFactory.CreateSpawner(spawnerData.position, spawnerData.Id, spawnerData.MonsterTypeID);
            }
        }

        private async Task InitSaveTrigger(LevelStaticData levelData) 
        {
            foreach (SaveTriggerData saveTriggerData in levelData.SaveTriggers)
            {
                await _gameFactory.CreateSaveTrigger(saveTriggerData.Position, saveTriggerData.Id);
            }
        }

        private LevelStaticData LevelStaticData() =>
            _staticDataService.ForLevel(SceneManager.GetActiveScene().name);
    }
}
