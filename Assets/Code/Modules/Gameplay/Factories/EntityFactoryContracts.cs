using System.Threading.Tasks;
using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface ILootSpawnFactory { UniTask<LootPiece> CreateLootFromPool(); }
    public interface IEnemySpawnFactory { Task<GameObject> CreateEnemies(MonsterTypeID type, Transform parent); }
    public interface IHeroFactory { UniTask<HeroFacade> CreateAsync(Vector3 position); }
    public interface IEnemyFactory
    {
        Task<GameObject> CreateAsync(MonsterTypeID type, Transform parent, HeroFacade hero, ILootSpawnFactory loot);
    }
    public interface ILootFactory
    {
        Task<LootPiece> CreateAsync();
        UniTask<LootPiece> SpawnAsync();
        UniTask WarmUpAsync();
        void DespawnAll();
    }
}
