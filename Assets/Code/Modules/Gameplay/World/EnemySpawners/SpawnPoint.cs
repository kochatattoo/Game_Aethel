using System;
using CodeBase.Data;
using CodeBase.Enemies;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Logic
{
    public class SpawnPoint : MonoBehaviour, ISavedProgress, IAsyncProgressReader, IPoolable
    {
        public MonsterTypeID MonsterTypeID;
        [SerializeField] private bool _slain;
        private IEnemySpawnFactory _factory;
        private EnemyDeath _enemyDeath;
        private int _generation;

        public bool Slain => _slain;
        public string Id { get; set; }
        public void Construct(IEnemySpawnFactory factory) => _factory = factory;
        public void LoadProgress(PlayerProgress progress) => LoadProgressAsync(progress).Forget(Debug.LogException);

        public async UniTask LoadProgressAsync(PlayerProgress progress)
        {
            var generation = ++_generation;
            DetachEnemy();
            _slain = progress.KillData.ClaeredSpawners.Contains(Id);
            if (_slain) return;

            var monster = await _factory.CreateEnemies(MonsterTypeID, transform);
            if (this == null || generation != _generation)
            {
                if (monster != null) Destroy(monster);
                return;
            }
            _enemyDeath = monster.GetComponent<EnemyDeath>();
            if (_enemyDeath == null) throw new InvalidOperationException("Enemy prefab has no EnemyDeath component");
            _enemyDeath.Happened += Slay;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            if (_slain && !progress.KillData.ClaeredSpawners.Contains(Id))
                progress.KillData.ClaeredSpawners.Add(Id);
        }

        private void Slay()
        {
            _slain = true;
            DetachEnemy(false);
        }

        private void DetachEnemy(bool destroy = true)
        {
            if (_enemyDeath == null) return;
            _enemyDeath.Happened -= Slay;
            if (destroy) Destroy(_enemyDeath.gameObject);
            _enemyDeath = null;
        }

        public void OnSpawned() => _slain = false;
        public void OnDespawned() { ++_generation; DetachEnemy(); }
        private void OnDestroy() { ++_generation; DetachEnemy(false); }
    }
}
