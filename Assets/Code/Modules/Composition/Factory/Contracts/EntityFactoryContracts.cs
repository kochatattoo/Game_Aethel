using System.Threading.Tasks;
using CodeBase.Enemies;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.Factory
{
    public interface ILootFactory
    {
        Task<LootPiece> CreateAsync();
        UniTask<LootPiece> SpawnAsync();
        UniTask WarmUpAsync();
        void DespawnAll();
    }
}
