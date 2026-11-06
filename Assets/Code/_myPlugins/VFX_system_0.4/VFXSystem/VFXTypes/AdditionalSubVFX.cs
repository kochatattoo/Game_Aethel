using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Компонент для второстепенных эффектов, таких как вспышки света (Light).
    /// Гарантирует корректное отключение источников света при возврате объекта в пул.
    /// </summary>
    public class AdditionalSubVFX : MonoBehaviour, ISubVFX
    {
        [Header("Components")]
        [SerializeField] 
        private Light _impactLight;

        [Header("Settings")]
        [SerializeField] 
        private float _duration = 0.2f;

        private readonly BoolReactiveProperty _isPlaying = new(false);
        private CancellationTokenSource _cts;

        public float Duration => _duration;

        public GameObject GameObject => gameObject;

        public BoolReactiveProperty IsPlayingObservable => _isPlaying;

        public void Play(float impactScale)
        {
            Stop();
            gameObject.SetActive(true);

            if (_impactLight != null)
            {
                _impactLight.enabled = true;
                // Масштабируем интенсивность или радиус в зависимости от силы удара
                _impactLight.intensity *= impactScale;

                LightLifecycleAsync(_cts.Token).Forget();
            }
        }

        public void ResetVFX() => Stop();

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_impactLight != null) 
                _impactLight.enabled = false;

            _isPlaying.Value = false;
            gameObject.SetActive(false);
        }

        private async UniTaskVoid LightLifecycleAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_duration), cancellationToken: token);

                if (_impactLight != null)
                    _impactLight.enabled = false;
            }
            catch (OperationCanceledException)
            {
                if (_impactLight != null) 
                    _impactLight.enabled = false;
            }
            finally
            {
                _isPlaying.Value = false;
            }
        }

        private void OnDestroy()
        {
            Stop();
            _isPlaying.Dispose();
        }
    }
}