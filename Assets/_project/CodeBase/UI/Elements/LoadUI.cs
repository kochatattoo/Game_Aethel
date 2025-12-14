using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.SaveLoad;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class LoadUI : SaveLoadUI
    {
        private IReloadService _reloadService;
        public Button LoadButton;

        public void Construct(IReloadService reloadService, IInputService inputService)
        {
            base.Construct(inputService);
            _reloadService = reloadService;
        }

        protected override void OnSubscribe()
        {
            LoadButton.onClick.AddListener(Reload);
        }

        protected override void CleanUp()
        {
            LoadButton.onClick.RemoveAllListeners();
        }

        private void Reload()
        {
            _reloadService.Reload();
        }
    }
}
