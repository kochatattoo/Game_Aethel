using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory: IService, IEnemySpawnFactory, ILootSpawnFactory
    {
        UniTask<HeroFacade> CreateHero(Vector3 at);
        Task<LootPiece> CreateLoot();
        Task<LootPiece> CreateLoot(string id);
        UniTask<LootPiece> CreateLootFromPool(string id);
        Task CreateSpawner(Vector3 at, string spawnerId, MonsterTypeID monsterTypeID);
        Task CreateTransferToPoint(LevelTransferData levelTransferData);
        Task CreateSaveTrigger(Vector3 at, string triggerId);
        UniTask WarmUpAsync();
        void CleanUp();
    }
}