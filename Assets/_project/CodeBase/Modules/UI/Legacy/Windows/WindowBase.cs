using CodeBase.Data;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows
{
    public abstract class WindowBase : MonoBehaviour, IPoolable
    {
        public Button CloseButton;

        protected IPersistentProgressService _progressService;
        protected PlayerProgress Progress => _progressService.Progress;

        public void Construct(IPersistentProgressService progressService)
        {
            _progressService = progressService;
        }

        public void OnSpawned()
        {
            CloseButton.onClick.AddListener(Close);

            Initialize();
            SubscribeUpdates();
        }

        public void OnDespawned()
        {
            Cleanup();
            CloseButton.onClick.RemoveListener(Close);
        }

        protected virtual void Initialize() { }

        protected virtual void SubscribeUpdates() { }

        protected virtual void Cleanup() { }

        protected abstract void Close();

    }
}
