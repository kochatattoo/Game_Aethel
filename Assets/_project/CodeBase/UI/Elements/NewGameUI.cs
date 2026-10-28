using CodeBase.Infrastructure.Services.SaveLoad;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class NewGameUI : MonoBehaviour
    {
        [SerializeField]
        private Button _newGameButton;
        private IReloadService _reloadService;

        public void Construct(IReloadService reloadService, UnityAction action = null)
        {
            _reloadService = reloadService;

            _newGameButton.AddListener(action);
            OnSubscribe();
        }

        public void OnSubscribe()
        {
            _newGameButton.AddListener(NewGame);
        }

        public void CleanUp()
        {
            _newGameButton?.RemoveAllListeners();
        }

        private void NewGame()
        {
            _reloadService.NewGame();
        }

        private void OnDisable()
        {
            CleanUp();
        }
    }
}
