using System;
using System.Threading.Tasks;
using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using UnityEngine;
using UnityEngine.AI;
using VFXSystem.Service;

namespace CodeBase.Infrastructure.Factory
{
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly IAsset _assets;
        private readonly IStaticDataService _catalog;
        private readonly IRandomService _random;
        private readonly IVFXFacade _vfx;
        public EnemyFactory(IAsset assets, IStaticDataService catalog, IRandomService random, IVFXFacade vfx)
        {
            _assets = assets;
            _catalog = catalog;
            _random = random;
            _vfx = vfx;
        }

        public async Task<GameObject> CreateAsync(MonsterTypeID type, Transform parent, HeroFacade hero, ILootSpawnFactory loot)
        {
            if (hero == null) throw new InvalidOperationException("Нельзя создать врага до создания героя");
            var data = _catalog.ForMonster(type);
            var prefab = await _assets.Load<GameObject>(data.PrefabReference);
            var enemy = UnityEngine.Object.Instantiate(prefab, parent.position, Quaternion.identity, parent);
            IHealth health = enemy.GetComponent<EnemyHealth>();
            health.Current = data.Hp;
            health.Max = data.Hp;
            enemy.GetComponent<ActorUI>().Construct(health);
            enemy.GetComponent<AgentMoveToHero>().Construct(hero.transform);
            enemy.GetComponent<EnemyVision>().Construct(hero.transform);
            enemy.GetComponent<NavMeshAgent>().speed = data.MoveSpeed;
            var drops = enemy.GetComponentInChildren<LootSpawner>();
            drops.Setloot(data.MinLoot, data.MaxLoot);
            drops.Construct(loot, _random);
            enemy.GetComponent<Attack>().Construct(hero.transform, hero.HeroDeath, _vfx);
            enemy.GetComponent<RotateToHero>()?.Consturct(hero.transform);
            var direction = hero.transform.position - enemy.transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > .001f) enemy.transform.rotation = Quaternion.LookRotation(direction);
            return enemy;
        }
    }
}
