using CodeBase.CharacterIntegration;
using UnityEngine;
using UnityEngine.UI;
namespace Code.UI.Elements
{
    public class OpenWindowButton : MonoBehaviour
    {
        [SerializeField] private Button Button;
        [SerializeField] private int WindowId;
        private CharacterMenuService _service;
        public void Construct(CharacterMenuService service){_service=service;Button.onClick.RemoveListener(Open);Button.onClick.AddListener(Open);}
        private void Open(){if(WindowId==2)_service.StartNewGame();else if(WindowId==3)_service.Quit();}
        private void OnDestroy(){if(Button!=null)Button.onClick.RemoveListener(Open);}
    }
}

