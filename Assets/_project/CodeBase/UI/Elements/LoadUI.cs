using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.SaveLoad;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class LoadUI : SaveLoadUI
    {
        public Button LoadButton;
        private IReloadService _reloadService;

        public void Construct(IReloadService reloadService, IInputService inputService, UnityAction action = null)
        {
            base.Construct(inputService);
            _reloadService = reloadService;

            LoadButton.AddListener(action);
        }

        protected override void OnSubscribe()
        {
            LoadButton.AddListener(Reload);
        }

        protected override void CleanUp()
        {
            LoadButton.RemoveAllListeners();
        }

        private void Reload()
        {
            _reloadService.Reload();
        }
    }
}
