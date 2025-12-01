
using CodeBase.StaticData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Infrastructure.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string StaticDataLevelsPath = "StaticData/levels";

        private Dictionary<string, LevelStaticData> _levels;

        public void Load()
        {
            _levels = Resources
                .LoadAll<LevelStaticData>(StaticDataLevelsPath)
                .ToDictionary(x => x.LevelKey, x => x);
        }

        public LevelStaticData ForLevel(string sceneKey) =>
             _levels.TryGetValue(sceneKey, out LevelStaticData staticData)
                ? staticData
                : null;
    }
}
