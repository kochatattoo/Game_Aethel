using CodeBase.Enemies;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.Factory
{
    public interface ILootSpawnFactory
    {
        UniTask<LootPiece> CreateLootFromPool();
    }
}
