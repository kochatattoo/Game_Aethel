using CodeBase.Data;
using CodeBase.Enemies;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic
{
    public class SpawnPoint : MonoBehaviour, ISavedProgress
    {
        public MonsterTypeID MonsterTypeID;
        private string _id;

        [SerializeField] private bool _slain;
        private IGameFactory _factory;
        private EnemyDeath _enemyDeath;
        public bool Slain => _slain;

        public string Id { get => _id; set => _id = value; }

        public void Construct(IGameFactory factory) =>
            _factory = factory;

        public void LoadProgress(PlayerProgress progress)
        {
            Debug.Log("Load Spawn Progress State");

            if (progress.KillData.ClaeredSpawners.Contains(_id))
                _slain = true;
            else
                Spawn();
        }

        private async void Spawn()
        {
            GameObject monster = await _factory.CreateEnemies(MonsterTypeID, transform);
            _enemyDeath = monster.GetComponent<EnemyDeath>();
            _enemyDeath.Happened += Slay;
        }

        private void Slay()
        {
            if (_enemyDeath != null)
                _enemyDeath.Happened -= Slay;

            _slain = true;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            if (_slain)
            {
                Debug.Log("Регестрируем выключеный спавнер");
                progress.KillData.ClaeredSpawners.Add(_id);
            }
        }
    }
}

