using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.UI.Elements;

namespace CodeBase.UI.Windows
{
    public class OptionWindow : WindowBase
    {
        public SaveUI saveUI;
        public LoadUI loadUI;

        public void Construct(ISaveLoadService saveLoadService,
                              IPersistentProgressService persistentProgress,
                              IReloadService reloadService,
                              IInputService inputService)
        {
            base.Construct(persistentProgress);
            saveUI.Construct(saveLoadService, inputService);
            loadUI.Construct(reloadService, inputService);
        }
    }
}
