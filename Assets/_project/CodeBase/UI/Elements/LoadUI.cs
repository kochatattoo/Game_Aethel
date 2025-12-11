using CodeBase.Infrastructure.Services.SaveLoad;

namespace CodeBase.UI.Elements
{
    public class LoadUI : SaveLoadUI
    {
        private IReloadService _reloadService;

        protected override void Start()
        {
            base.Start();

        }

        protected void Update()
        {
            //if (_inputService.IsLoadButtonUp())
            //    _reloadService.Reload();
        }
    }
}
