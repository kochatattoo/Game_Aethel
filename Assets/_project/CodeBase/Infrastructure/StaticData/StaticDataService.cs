
using CodeBase.StaticData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Infrastructure.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string StaticDataLevelsPath = "StaticData/levels";
        private const string StaticDataMonstersPath = "StaticData/Monsters";

        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<MonsterTypeID, MonsterStaticData> _monsters;

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
                : null;

        public MonsterStaticData ForMonster(MonsterTypeID typeId) =>
         _monsters.TryGetValue(typeId, out MonsterStaticData staticData)
             ? staticData
             : null;
    }
}
