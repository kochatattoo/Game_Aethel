using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Управляет визуальным отображением декалей (отпечатков) на поверхностях.
    /// Отвечает за логику появления, масштабирования от силы удара и последующего скрытия.
    /// </summary>
    public class DecalSubVFX : MonoBehaviour, ISubVFX
    {
        [SerializeField]
        private float _duration = 5f;
        [SerializeField]
        private float _fadeOutTime = 1.5f; // Время плавного исчезновения в конце
        [SerializeField]
        private DecalProjector _projector;

        private readonly BoolReactiveProperty _isPlaying = new(false);
        private CancellationTokenSource _cts;

        public GameObject GameObject => gameObject;

        public float Duration => _duration; // Время до исчезновения

        public BoolReactiveProperty IsPlayingObservable => _isPlaying;

        public void Play(float impactScale)
        {
            Stop();

            _cts = new CancellationTokenSource();
            _isPlaying.Value = true;

            transform.localScale = Vector3.one * impactScale;
            gameObject.SetActive(true);

            if (_projector != null)
            {
                _projector.fadeFactor = 1f;
                _projector.size = new Vector3(impactScale, impactScale, _projector.size.z);
            }

            DecalLifeCycleAsync(_cts.Token).Forget();
        }

        public void ResetVFX() => Stop();

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_projector != null) 
                _projector.fadeFactor = 0f;

            _isPlaying.Value = false;
            gameObject.SetActive(false);
        }

        private async UniTaskVoid DecalLifeCycleAsync(CancellationToken token)
        {
            try
            {
                float waitTime = Mathf.Max(0, _duration - _fadeOutTime);
                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);

                if (_projector != null && _fadeOutTime > 0)
                {
                    float elapsed = 0;
                    while (elapsed < _fadeOutTime)
                    {
                        elapsed += Time.deltaTime;
                        _projector.fadeFactor = Mathf.Lerp(1f, 0f, elapsed / _fadeOutTime);
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
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
