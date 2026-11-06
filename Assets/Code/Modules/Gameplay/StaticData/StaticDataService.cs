using CodeBase.StaticData;


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Services.StaticData
{
    public class StaticDataService : IStaticDataService, IInitializable
    {
        private const string StaticDataLevelsPath = "StaticData/levels";
        private const string StaticDataMonstersPath = "StaticData/monsters";

        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<MonsterTypeID, MonsterStaticData> _monsters;

        public void Initialize()
        {
            Load();
        }

        public void Load()
        {
            _monsters = Resources
                .LoadAll<MonsterStaticData>(StaticDataMonstersPath)
                .ToDictionary(x => x.MonsterTypeId, x => x);

            _levels = Resources
                .LoadAll<LevelStaticData>(StaticDataLevelsPath)
                .ToDictionary(x => x.LevelKey, x => x);

        }

        public LevelStaticData ForLevel(string sceneKey) =>
             _levels.TryGetValue(sceneKey, out LevelStaticData staticData)
                ? staticData
                : throw new System.InvalidOperationException("Не найдены данные уровня: " + sceneKey);

        public MonsterStaticData ForMonster(MonsterTypeID typeId) =>
         _monsters.TryGetValue(typeId, out MonsterStaticData staticData)
             ? staticData
             : throw new System.InvalidOperationException("Не найдены данные монстра: " + typeId);

    }
}
