using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class LootSpawner : MonoBehaviour
    {
        private ILootSpawnFactory _factory;
        private IRandomService _random;
        private int _lootMin;
        private int _lootMax;
        private EnemyActor _actor;

        public void Construct(ILootSpawnFactory factory, IRandomService random)
        {
            _factory = factory;
            _random = random;
        }

        public void Initialize(EnemyActor actor)
        {
            if (_actor != null) _actor.Defeated -= OnDeath;
            _actor = actor;
            if (_actor != null) _actor.Defeated += OnDeath;
        }

        private void OnDestroy()
        {
            if (_actor != null) _actor.Defeated -= OnDeath;
        }

        public void Setloot(int min, int max)
        {
            _lootMin = min;
            _lootMax = max;
        }

        private void OnDeath() => SpawnLootAsync(transform.position).Forget(Debug.LogException);

        private async UniTask SpawnLootAsync(Vector3 position)
        {
            var loot = await _factory.CreateLootFromPool();
            if (this == null)
            {
                loot.Release();
                return;
            }
            loot.transform.position = position;
            loot.Initialize(new Loot { Value = _random.Next(_lootMin, _lootMax) });
        }
    }
}
