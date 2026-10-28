using UnityEngine;
using UnityEngine.UI;
namespace Code.UI.MainMenuElements
{
    public class ExitButton : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        // Кнопками управляет владелец окна, чтобы закрытие атрибутов не завершало игру.
    }
}

