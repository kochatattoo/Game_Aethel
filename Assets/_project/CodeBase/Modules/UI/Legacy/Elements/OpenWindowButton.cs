using CodeBase.UI.Services.Windows;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class OpenWindowButton : MonoBehaviour
    {
        public Button Button;
        public WindowId WindowId;
        private IWindowService _windows;

        public void Construct(IWindowService windows) => _windows = windows;
        private void OnEnable() => Button.onClick.AddListener(Open);
        private void OnDisable() => Button.onClick.RemoveListener(Open);
        private void Open() => _windows.OpenAsync(WindowId).Forget(Debug.LogException);
    }
}
