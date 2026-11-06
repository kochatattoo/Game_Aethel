using Zenject;
using UnityEngine;
using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;


namespace AudioSystem.Banks
{
    public class SceneBankLoader : MonoBehaviour
    {
        [SerializeField]
        private BankSceneMapping _sceneMapping;

        private IBankLoader _bankLoader;
        private CancellationTokenSource _cts;
        private IEnumerable<WwiseBankAsset> _banksForThisScene;

        [Inject]
        private void Construct(IBankLoader bankLoader)
        {
            _bankLoader = bankLoader;

            string currentSceneName = gameObject.scene.name;
            var mappingData = _sceneMapping.Banks.FirstOrDefault(data => data.SceneName == currentSceneName);
            if (mappingData != null)
            {
                _banksForThisScene = mappingData.BankSceneMapping;
            }
            else
            {
                Debug.LogWarning($"[Audio] No bank mapping found for scene '{currentSceneName}'.");
                _banksForThisScene = Enumerable.Empty<WwiseBankAsset>();
            }
        }

        private void Start()
        {
            if (_banksForThisScene.Any())
            {
                _cts = new CancellationTokenSource();
                LoadBanksAsync(_cts.Token).Forget();
            }
        }

        private async UniTaskVoid LoadBanksAsync(CancellationToken cancellationToken)
        {
            var progress = new Progress<float>(p => OnProgress(p));

            try
            {
                await _bankLoader.LoadBanksAsync(_banksForThisScene, progress, cancellationToken);

                if (_bankLoader is BankLoaderService service)
                    service.NotifySceneBanksLoaded();

                Debug.Log($"[Audio] Banks for scene '{gameObject.scene.name}' loaded.");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[Audio] Bank loading was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Audio] Failed to load banks for scene '{gameObject.scene.name}': {ex}");
            }
        }

        private void OnProgress(float progress)
        {
            // Опционально: обновить UI загрузки
            Debug.Log($"[Audio] Loading scene banks: {progress:P0}");
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (_banksForThisScene != null && _banksForThisScene.Any())
            {
                _bankLoader.UnloadBanksAsync(_banksForThisScene).Forget();
            }
        }
    }
}
