using CodeBase.StaticData;
using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string StaticDataLevelsPath = "StaticData/levels";
        private const string StaticDataMonstersPath = "StaticData/monsters";
        private const string StaticDataWindowsPath = "StaticData/ui/WindowStaticData";

        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<MonsterTypeID, MonsterStaticData> _monsters;
        private Dictionary<WindowId, WindowConfig> _windowConfigs;

        public void Load()
        {
            _monsters = Resources
                .LoadAll<MonsterStaticData>(StaticDataMonstersPath)
                .ToDictionary(x => x.MonsterTypeId, x => x);

            _levels = Resources
                .LoadAll<LevelStaticData>(StaticDataLevelsPath)
                .ToDictionary(x => x.LevelKey, x => x);

            _windowConfigs = Resources
                .Load<WindowStaticData>(StaticDataWindowsPath)
                .Configs
                .ToDictionary(x => x.WindowId, x => x);
        }

        public LevelStaticData ForLevel(string sceneKey) =>
             _levels.TryGetValue(sceneKey, out LevelStaticData staticData)
                ? staticData
                : null;

        public MonsterStaticData ForMonster(MonsterTypeID typeId) =>
         _monsters.TryGetValue(typeId, out MonsterStaticData staticData)
             ? staticData
             : null;

        public WindowConfig ForWindow(WindowId windowId) =>
             _windowConfigs.TryGetValue(windowId, out WindowConfig windowConfig)
                ? windowConfig
                : null;
    }
}
