using CodeBase.UI.Services.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class OpenWindowButton : MonoBehaviour
    {
        public Button Button;
        public WindowId WindowId;
        private IWindowService _windowService;

        public void Construct(IWindowService windowService) =>
            _windowService = windowService;

        private void Awake() =>
            Button.AddListener(OpenAsync);

        private void OnDisable() => 
            Button.RemoveListener(OpenAsync);

        private void Open()
        {
            _windowService.Open(WindowId);
        }

        private async void OpenAsync()
        {
           await _windowService.OpenAsync(WindowId);
        }
    }
}
