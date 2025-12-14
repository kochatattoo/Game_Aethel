using CodeBase.StaticData;
using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;

namespace CodeBase.Infrastructure.Services.StaticData
{
    public interface IStaticDataService : IService
    {
        void Load();
        LevelStaticData ForLevel(string sceneKey);
        MonsterStaticData ForMonster(MonsterTypeID typeId);
        WindowConfig ForWindow(WindowId shop);
    }
}