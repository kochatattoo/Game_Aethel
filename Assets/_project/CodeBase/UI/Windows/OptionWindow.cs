using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.UI.Elements;

namespace CodeBase.UI.Windows
{
    public class OptionWindow : WindowBase, IPoolable<OptionWindow>
    {
        public SaveUI saveUI;
        public LoadUI loadUI;
        private IPool<OptionWindow> _pool;

        public void Construct(IPersistentProgressService persistentProgress,
                              ISaveLoadService saveLoadService,
                              IReloadService reloadService,
                              IInputService inputService)
        {
            base.Construct(persistentProgress);
            saveUI.Construct(saveLoadService, inputService);
            loadUI.Construct(reloadService, inputService, Close);
        }

        public void SetPool(IPool<OptionWindow> pool)
        {
            _pool = pool;
        }

        protected override void Close()
        {
            _pool.Despawn(this);
        }
    }
}
