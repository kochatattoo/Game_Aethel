using CodeBase.Infrastructure.Services.ObjectPool;
using UnityEngine;
namespace PetProject.Architecture.Tests
{
    public sealed class PoolProbe : MonoBehaviour, IPoolable<PoolProbe>
    {
        public int Spawns, Returns;
        public IPool<PoolProbe> Pool;
        public void SetPool(IPool<PoolProbe> pool) => Pool = pool;
        public void OnSpawned() => ++Spawns;
        public void OnDespawned() => ++Returns;
    }
}
