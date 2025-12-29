using UnityEngine.Events;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public static class UIExtensions
    {
        public static Button AddListener(this Button button, UnityAction action)
        { 
            button.onClick.AddListener(action); 
            return button;
        }

        public static Button RemoveListener(this Button button, UnityAction action)
        {
            button.onClick.RemoveListener(action);
            return button;
        }

        public static Button RemoveAllListeners(this Button button)
        {
            button.onClick.RemoveAllListeners();
            return button;
        }
    }
}
