using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using Assets._project.CodeBase.Infrastructure.Services.Input;
using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Levels;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using CodeBase.UI.Services.Windows;
using Cysharp.Threading.Tasks;
using Infrastructure.AudioSystem;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using VFXSystem.Processors;
using VFXSystem.Service;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAsset _assets;
        private readonly IStaticDataService _staticDataService;
        private readonly IPersistentProgressService _progressService;
        private readonly IRandomService _randomService;
        private readonly ILevelTransferService _levelTransfer;
        private readonly IWindowService _windowService;
        private readonly ISaveLoadService _saveLoad;
        private readonly IPoolService _poolService;
        private readonly IBlackboardService _blackboardService;
        private readonly IInputHandlerService _inputHandlerService;
        private readonly IAudioFacade _audioFacade;
        private readonly IVFXFacade _vFXFacade;

        private HeroFacade HeroFacade { get; set; }
        public List<ISavedProgressReader> ProgressReaders { get; } = new List<ISavedProgressReader>();
        public List<ISavedProgress> ProgressWriters { get; } = new List<ISavedProgress>();

        public GameFactory(IAsset asset,
                           IStaticDataService staticData,
                           IPersistentProgressService progressService,
                           IRandomService randomService,
                           ILevelTransferService levelTransfer,
                           IWindowService windowService,
                           ISaveLoadService saveLoad,
                           IPoolService poolService,
                           IBlackboardService blackboardService,
                           IInputHandlerService inputHandlerService,
                           IAudioFacade audioFacade,
                           IVFXFacade vFXFacade)
        {
            _assets = asset;
            _staticDataService = staticData;
            _progressService = progressService;
            _randomService = randomService;
            _levelTransfer = levelTransfer;
            _windowService = windowService;
            _saveLoad = saveLoad;
            _poolService = poolService;
            _blackboardService = blackboardService;
            _inputHandlerService = inputHandlerService;
            _audioFacade = audioFacade;
            _vFXFacade = vFXFacade;
        }

        public async UniTask WarmUpAsync()
        {
            var prefabLoot =  _assets.Load<GameObject>(AssetAddress.Loot);
            var prefabSpawner =  _assets.Load<GameObject>(AssetAddress.Spawner);


            await Task.WhenAll(prefabLoot, prefabSpawner);

            LootPiece loot = (await prefabLoot).GetComponent<LootPiece>();
            SpawnPoint spawn = (await prefabSpawner).GetComponent<SpawnPoint>();

            var warmLootTask = _poolService.AddPoolToContainerAsync<LootPiece>(loot, 5);
            var warmSpawnerTask = _poolService.AddPoolToContainerAsync<SpawnPoint>(spawn, 5);

            await UniTask.WhenAll(warmLootTask, warmSpawnerTask);
        }

        public async Task<GameObject> CreateHud()
        {
            GameObject hud = await InstantiateRegisteredAsync(AssetAddress.HUDPath);
            hud.GetComponentInChildren<LootCounter>()
                .Construct(_progressService.Progress.WorldData);

            foreach (OpenWindowButton openWindowButton in hud.GetComponentsInChildren<OpenWindowButton>())
            {
                openWindowButton.Construct(_windowService);
            }

            return hud;
        }

        public async UniTask<HeroFacade> CreateHero(Vector3 at)
        {
            GameObject HeroGameObject = await InstantiateRegisteredAsync(AssetAddress.HeroPath, at);

            HeroFacade = HeroGameObject.GetComponent<HeroFacade>();
            HeroFacade.Construct(_inputHandlerService, _blackboardService, _audioFacade, _vFXFacade);
            HeroFacade.Initialize();

            //HeroGameObject.GetComponent<HeroMove>()
            //   .Construct(_inputService);

            //HeroGameObject .GetComponent<HeroPathFollower>()
            //    .Construct(_inputService);

            //HeroGameObject.GetComponent<HeroAttack>() 
            //   .Construct(_inputService);

            return HeroFacade;
        }

        public async Task<GameObject> CreateEnemies(MonsterTypeID typeId, Transform parent)
        {
            MonsterStaticData monsterData = _staticDataService.ForMonster(typeId);

            GameObject prefab = await _assets.Load<GameObject>(monsterData.PrefabReference);
            GameObject monster = Object.Instantiate(prefab, parent.position, Quaternion.identity, parent);
            

            IHealth health = monster.GetComponent<EnemyHealth>();
            health.Current = monsterData.Hp;
            health.Max = monsterData.Hp;

            monster.GetComponent<ActorUI>().Construct(health);
            monster.GetComponent<AgentMoveToHero>().Construct(HeroFacade.transform);
            monster.GetComponent<EnemyVision>().Construct(HeroFacade.transform);
            monster.GetComponent<NavMeshAgent>().speed = monsterData.MoveSpeed;

            LootSpawner lootSpawner = monster.GetComponentInChildren<LootSpawner>();
            lootSpawner.Setloot(monsterData.MinLoot, monsterData.MaxLoot);
            lootSpawner.Construct(this, _randomService);

            Attack attack = monster.GetComponent<Attack>();
            attack.Construct(HeroFacade.transform, HeroFacade.HeroDeath, _vFXFacade);
            //TODO: Вот тут я еще конфиг передавал MonsterStaticData - полдумать как объединить с конфигом EnemyAttackConfig

            monster.GetComponent<RotateToHero>()?.Consturct(HeroFacade.transform);

            FaceTarget(monster.transform, HeroFacade.transform.position);

            return monster;
        }

        public async Task<LootPiece> CreateLoot()
        {
            GameObject prefab = await InstantiateRegisteredAsync(AssetAddress.Loot);
            LootPiece lootPiece = prefab.GetComponent<LootPiece>();

            lootPiece.Construct(_progressService.Progress.WorldData, this);

            return lootPiece;
        }

        public async Task<LootPiece> CreateLoot(string id)
        {
            LootPiece lootPiece = await CreateLoot();

            lootPiece.SetId(id);

            return lootPiece;
        }

        public async UniTask<LootPiece> CreateLootFromPool()
        {
            IPool<LootPiece> pool = _poolService.GetPool<LootPiece>();
            LootPiece lootObject = await pool.SpawnExpandableAsync(w => w.Construct(_progressService.Progress.WorldData, this));
            Register(lootObject);

            lootObject.Construct(_progressService.Progress.WorldData, this);

            return lootObject;
        }

        public async UniTask<LootPiece> CreateLootFromPool(string id)
        {
            LootPiece lootPiece = await CreateLootFromPool();

            lootPiece.SetId(id);

            return lootPiece;
        }

        public async Task CreateSpawner(Vector3 at, string spawnerId, MonsterTypeID monsterTypeID)
        {
            GameObject prefab = await InstantiateRegisteredAsync(AssetAddress.Spawner, at);
            SpawnPoint spawner= prefab.GetComponent<SpawnPoint>();

            spawner.Construct(this);
            spawner.Id = spawnerId;
            spawner.MonsterTypeID = monsterTypeID;
        }

        public async Task CreateSaveTrigger(Vector3 at, string triggerId)
        {
            GameObject prefab = await InstantiateRegisteredAsync(AssetAddress.SaveTrigger, at);
            SaveTrigger saveTrigger = prefab.GetComponent<SaveTrigger>();

            saveTrigger.Construct(_saveLoad);
            saveTrigger.Id = triggerId;
        }

        public async Task CreateTransferToPoint(LevelTransferData levelTransferData)
        {
            GameObject levelTransferTrigger = await InstantiateRegisteredAsync(AssetAddress.TransferToPoint, levelTransferData.TransferToPosition);
            levelTransferTrigger.GetComponent<LevelTransferTrigger>()
              .Construct(_levelTransfer, levelTransferData.LevelTo);
        }

        public void CleanUp()
        {
            ProgressReaders.Clear();
            ProgressWriters.Clear();

            _assets.CleanUp();
        }

        private void Register(ISavedProgressReader progressReader)
        {
            if (progressReader is ISavedProgress progressWriter)
                ProgressWriters.Add(progressWriter);

            ProgressReaders.Add(progressReader);
        }

        private async Task<GameObject> InstantiateRegisteredAsync(string path, Vector3 at)
        {
            GameObject gameObject = await _assets.Instantiate(path, at);
            RegisterProggressWatchers(gameObject);
            return gameObject;
        }

        private async Task<GameObject> InstantiateRegisteredAsync(string path)
        {
            GameObject gameObject = await _assets.Instantiate(path);
            RegisterProggressWatchers(gameObject);
            return gameObject;
        }

        private void RegisterProggressWatchers(GameObject gameObject)
        {
            foreach (ISavedProgressReader progressReader in gameObject.GetComponentsInChildren<ISavedProgressReader>())
            {
                Register(progressReader);
            }
        }

        private void FaceTarget(Transform prefab, Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - prefab.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                prefab.rotation = Quaternion.LookRotation(dir);
        }
    }
}
