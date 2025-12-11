using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IInputService _inputService;
        private readonly IAsset _assets;
        private readonly IStaticDataService _staticDataService;
        private readonly IPersistentProgressService _progressService;

        private GameObject HeroGameObject { get; set; }
        public List<ISavedProgressReader> ProgressReaders { get; } = new List<ISavedProgressReader>();
        public List<ISavedProgress> ProgressWriters { get; } = new List<ISavedProgress> { };

        public GameFactory(IInputService inputService, IAsset asset, IStaticDataService staticData, IPersistentProgressService progressService)
        {
            _inputService = inputService;
            _assets = asset;
            _staticDataService = staticData;
            _progressService = progressService;
        }
        public async Task WarmUp()
        {
            await _assets.Load<GameObject>(AssetAddress.Loot);
            await _assets.Load<GameObject>(AssetAddress.Spawner);
        }

        public async Task<GameObject> CreateHero(Vector3 at)
        {
            HeroGameObject = await InstantiateRegisteredAsync(AssetAddress.HeroPath, at);

            HeroGameObject.GetComponent<HeroMove>()
               .Construct(_inputService);
            HeroGameObject.GetComponent<HeroAttack>() 
               .Construct(_inputService);

            return HeroGameObject;
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
            monster.GetComponent<AgentMoveToHero>().Construct(HeroGameObject.transform);
            monster.GetComponent<NavMeshAgent>().speed = monsterData.MoveSpeed;

            //LootSpawner lootSpawner = monster.GetComponentInChildren<LootSpawner>();
            //lootSpawner.Setloot(monsterData.MinLoot, monsterData.MaxLoot);
            //lootSpawner.Construct(this, _randomService);

            Attack attack = monster.GetComponent<Attack>();
            attack.Construct(HeroGameObject.transform, HeroGameObject.GetComponent<HeroDeath>());
            attack.Damage = monsterData.Damage;
            attack.Cleavage = monsterData.Cleavage;
            attack.EffectiveDistance = monsterData.EffectiveDistance;

            monster.GetComponent<RotateToHero>()?.Consturct(HeroGameObject.transform);

            FaceTarget(monster.transform, HeroGameObject.transform.position);

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

        public async Task CreateSpawner(Vector3 at, string spawnerId, MonsterTypeID monsterTypeID)
        {
            GameObject prefab = await InstantiateRegisteredAsync(AssetAddress.Spawner, at);
            SpawnPoint spawner= prefab.GetComponent<SpawnPoint>();

            spawner.Construct(this);
            spawner.Id = spawnerId;
            spawner.MonsterTypeID = monsterTypeID;
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
