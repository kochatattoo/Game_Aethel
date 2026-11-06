using Cysharp.Threading.Tasks;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Factory;
using System;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Processors
{
    // На данный момент нигде не используется, но может потребоваться
    // Контроллер для оркестарции АудиоМейкера
    public class AudioMakerStateController : IDisposable
    {
        private readonly IAudioMaker _maker;
        private readonly IAudioFacade _audioFacade;
        private readonly Transform _attachPoint;
        private readonly CompositeDisposable _disposables = new();

        private readonly Dictionary<AudioEventAsset, StateEntry> _activeStates = new();

        public AudioMakerStateController(IAudioMaker maker, IAudioFacade audioFacade)
        {
            _maker = maker;
            _audioFacade = audioFacade;
            _attachPoint = maker.AudioMakerObject.transform;
        }

        public void EnterState(AudioEventAsset eventAsset, AudioParameterAsset parameterAsset, float fadeDuration, bool isLoop)
        {
            if (eventAsset == null)
                return;

            if (_activeStates.TryGetValue(eventAsset, out var entry))
            {
                entry.Count++;
                entry.CancelFadeOut?.Cancel();
                entry.CancelFadeOut = null;
                entry.IsLoop = isLoop;
            }
            else
            {
                var request = _audioFacade.PlayAttached(eventAsset, _attachPoint);
                request.Play();

                var updateSub = Observable.EveryUpdate()
                    .Subscribe(_ => UpdateParameter(eventAsset, parameterAsset));

                entry = new StateEntry
                {
                    Count = 1,
                    Request = request,
                    UpdateSubscription = updateSub,
                    CancelFadeOut = null,
                    ParameterAsset = parameterAsset,
                    IsLoop = isLoop
                };
                _activeStates[eventAsset] = entry;
            }

            UpdateParameter(eventAsset, parameterAsset);
        }

        public void ExitState(AudioEventAsset eventAsset, float fadeDuration, bool isLoop)
        {
            if (eventAsset == null || !_activeStates.TryGetValue(eventAsset, out var entry))
                return;

            entry.Count--;
            if (entry.Count > 0)
                return;

            if (entry.IsLoop)
            {
                FadeOutAndCleanup(eventAsset, entry, fadeDuration).Forget();
            }
            else
            {
                entry.UpdateSubscription?.Dispose();
                _audioFacade.SetParameter(entry.ParameterAsset, 0f);
                entry.Request.Stop();
                _activeStates.Remove(eventAsset);
            }
        }

        private async UniTaskVoid FadeOutAndCleanup(AudioEventAsset eventAsset, StateEntry entry, float fadeDuration)
        {

            entry.UpdateSubscription?.Dispose();
            entry.CancelFadeOut = new CancellationTokenSource();
            var token = entry.CancelFadeOut.Token;

            float startValue = _maker.CurrentSpeed;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                if (token.IsCancellationRequested)
                    return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float value = Mathf.Lerp(startValue, 0f, t);
                if (entry.ParameterAsset != null)
                    _audioFacade.SetParameter(entry.ParameterAsset, value);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (entry.ParameterAsset != null)
                _audioFacade.SetParameter(entry.ParameterAsset, 0f);

            entry.Request.Stop();
            _activeStates.Remove(eventAsset);
            entry.CancelFadeOut?.Dispose();
        }

        private void UpdateParameter(AudioEventAsset eventAsset, AudioParameterAsset parameterAsset)
        {
            if (parameterAsset == null)
                return;

            float speed = _maker.CurrentSpeed;
            _audioFacade.SetParameter(parameterAsset, speed);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            foreach (var kvp in _activeStates)
            {
                kvp.Value.UpdateSubscription?.Dispose();
                kvp.Value.Request.Stop();
            }
            _activeStates.Clear();
        }

        private class StateEntry
        {
            public int Count;
            public AudioRequest Request;
            public IDisposable UpdateSubscription;
            public CancellationTokenSource CancelFadeOut;
            public AudioParameterAsset ParameterAsset;
            public bool IsLoop;
        }
    }
}
