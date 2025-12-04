using CodeBase.StaticData;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory
    {
        Task<GameObject> CreateHero(Vector3 at);
        Task<GameObject> CreateEnemies(MonsterTypeID ID, Transform parent);
        Task CreateSpawner(Vector3 at, string spawnerId, MonsterTypeID monsterTypeID);
    }
}