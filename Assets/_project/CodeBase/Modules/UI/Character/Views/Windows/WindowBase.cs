using System;
using UnityEngine;
using UnityEngine.UI;
namespace Code.UI.Windows
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] private Button CloseButton;
        public event Action Closed;
        protected virtual void Awake(){if(CloseButton!=null)CloseButton.onClick.AddListener(Close);}
        public void Close(){Closed?.Invoke();Destroy(gameObject);}
        protected virtual void Cleanup(){}
        protected virtual void OnDestroy(){if(CloseButton!=null)CloseButton.onClick.RemoveListener(Close);Cleanup();}
    }
}

