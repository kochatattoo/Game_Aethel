using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.UI.Elements;
using UnityEngine;

namespace CodeBase.UI.Windows
{
    public class OptionWindow : WindowBase, IPoolable<OptionWindow>
    {
        [SerializeField]
        private SaveUI saveUI;
        [SerializeField]
        private LoadUI loadUI;
        [SerializeField]
        private NewGameUI newGameUI;

        private IPool<OptionWindow> _pool;

        public void Construct(IPersistentProgressService persistentProgress,
                              ISaveLoadService saveLoadService,
                              IReloadService reloadService,
                              IInputService inputService)
        {
            base.Construct(persistentProgress);
            saveUI.Construct(saveLoadService, inputService);
            loadUI.Construct(reloadService, inputService, Close);
            newGameUI.Construct(reloadService, Close);
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
