using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.SaveLoad;
using System;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class SaveUI : SaveLoadUI
    {
        public Button SaveButton; 

        private ISaveLoadService _saveLoadService;

        public void Construct(ISaveLoadService saveLoadService, IInputService inputService)
        {
            base.Construct(inputService);
            _saveLoadService = saveLoadService;
        }

        protected override void OnSubscribe()
        {
            SaveButton.onClick.AddListener(SaveProgress);
        }

        protected override void CleanUp()
        {
            SaveButton.onClick.RemoveAllListeners();
        }

        private void SaveProgress()
        {
            _saveLoadService.SaveProgress();
        }
    }
}
