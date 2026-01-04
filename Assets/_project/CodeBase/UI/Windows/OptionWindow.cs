using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.UI.Elements;

namespace CodeBase.UI.Windows
{
    public class OptionWindow : WindowBase
    {
        public SaveUI saveUI;
        public LoadUI loadUI;

        public void Construct(IPersistentProgressService persistentProgress,
                              IPoolService poolService,
                              ISaveLoadService saveLoadService,
                              IReloadService reloadService,
                              IInputService inputService)
        {
            base.Construct(persistentProgress, poolService);
            saveUI.Construct(saveLoadService, inputService);
            loadUI.Construct(reloadService, inputService);
        }

        protected override void Close()
        {
            _poolService.GetPool<OptionWindow>().Despawn(this);
        }
    }
}
