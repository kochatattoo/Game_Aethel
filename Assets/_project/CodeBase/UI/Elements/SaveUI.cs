using CodeBase.Infrastructure.Services.SaveLoad;

namespace CodeBase.UI.Elements
{
    public class SaveUI : SaveLoadUI
    {
        private ISaveLoadService _saveLoadService;

        protected override void Start()
        {
            base.Start();
        }

        protected void Update()
        {
            //if (_inputService.IsSaveButtonUp())
            //    _saveLoadService.SaveProgress();
        }
    }
}
