using CodeBase.Infrastructure.Services;
using System;
using UnityEngine;

namespace CodeBase.UI.Elements
{
    public abstract class SaveLoadUI : MonoBehaviour
    {
        protected IInputService _inputService;

        public void Construct(IInputService inputService)
        {
           _inputService = inputService;
        }

        private void Start()
        {
            OnSubscribe();
        }

        private void OnDisable()
        {
            CleanUp();
        }

        protected virtual void OnSubscribe() { }

        protected virtual void CleanUp() { }
    }
}
