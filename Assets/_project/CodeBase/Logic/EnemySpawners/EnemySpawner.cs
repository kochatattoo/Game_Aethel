using CodeBase.Enemy;
using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic
{
    public class SpawnPoint : MonoBehaviour
    {
        public MonsterTypeID MonsterTypeID;
        private string _id;

        [SerializeField] private bool _slain;
        private IGameFactory _factory;
        private EnemyDeath _enemyDeath;
        public bool Slain => _slain;

        public string Id { get => _id; set => _id = value; }

        public void Construct(IGameFactory factory) 
        {
            _factory = factory;
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
    }
}
