using CodeBase.Enemies;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService, ILootSpawnFactory
    {
        Task<LootPiece> CreateLoot();
        Task<LootPiece> CreateLoot(string id);
        UniTask<LootPiece> CreateLootFromPool(string id);
        Task CreateTransferToPoint(LevelTransferData levelTransferData);
        Task CreateSaveTrigger(Vector3 at, string triggerId);
        UniTask WarmUpAsync();
        void CleanUp();
    }
}
