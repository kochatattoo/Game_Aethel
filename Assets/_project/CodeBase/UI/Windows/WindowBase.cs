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
            CloseButton.AddListener(() => Close());

            Initialize();
            SubscribeUpdates();
        }

        public void OnDespawned()
        {
            Cleanup();
            CloseButton.RemoveAllListeners();
        }

        //private void Awake() =>
        //    OnAwake();

        //private void Start()
        //{
        //    Initialize();
        //    SubscribeUpdates();
        //}

        //private void OnDestroy()
        //{
        //    Cleanup();
        //}

        //protected virtual void OnAwake() =>
        //    CloseButton.AddListener(() => Destroy(gameObject));

        protected virtual void Initialize() { }

        protected virtual void SubscribeUpdates() { }

        protected virtual void Cleanup() { }

        protected abstract void Close();

    }
}
