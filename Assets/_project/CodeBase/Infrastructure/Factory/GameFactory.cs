using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Logic;
using CodeBase.StaticData;
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

        private GameObject HeroGameObject { get; set; }
        public List<ISavedProgressReader> ProgressReaders { get; } = new List<ISavedProgressReader>();
        public List<ISavedProgress> ProgressWriters { get; } = new List<ISavedProgress> { };

        public GameFactory(IInputService inputService, IAsset asset, IStaticDataService staticData)
        {
            _inputService = inputService;
            _assets = asset;
            _staticDataService = staticData;
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

            //monster.GetComponent<ActorUI>().Construct(health);
            monster.GetComponent<AgentMoveToHero>().Construct(HeroGameObject.transform);
            monster.GetComponent<NavMeshAgent>().speed = monsterData.MoveSpeed;

            //LootSpawner lootSpawner = monster.GetComponentInChildren<LootSpawner>();
            //lootSpawner.Setloot(monsterData.MinLoot, monsterData.MaxLoot);
            //lootSpawner.Construct(this, _randomService);

            Attack attack = monster.GetComponent<Attack>();
            attack.Construct(HeroGameObject.transform);
            attack.Damage = monsterData.Damage;
            attack.Cleavage = monsterData.Cleavage;
            attack.EffectiveDistance = monsterData.EffectiveDistance;

            monster.GetComponent<RotateToHero>()?.Consturct(HeroGameObject.transform);

            return monster;
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

        private void RegisterProggressWatchers(GameObject gameObject)
        {
            foreach (ISavedProgressReader progressReader in gameObject.GetComponentsInChildren<ISavedProgressReader>())
            {
                Register(progressReader);
            }
        }
    }
}
