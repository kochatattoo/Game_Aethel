using CodeBase.Enemies;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory
    {
        List<ISavedProgressReader> ProgressReaders { get; }
        List<ISavedProgress> ProgressWriters { get; }
        Task<GameObject> CreateHero(Vector3 at);
        Task<GameObject> CreateHud();
        Task<GameObject> CreateEnemies(MonsterTypeID ID, Transform parent);
        Task<LootPiece> CreateLoot();
        Task<LootPiece> CreateLoot(string id);
        Task CreateSpawner(Vector3 at, string spawnerId, MonsterTypeID monsterTypeID);
        Task CreateTransferToPoint(LevelTransferData levelTransferData);
        Task WarmUp();
    }
}