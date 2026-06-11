using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Processors;
using Infrastructure.AudioSystem.Events;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Zenject;

namespace Infrastructure.AudioSystem
{
    // Новый сервис в системе, на данный момент нигде не используется
    // Служит для оркестрации АудиоМейкеров
    public class AudioMakerStateService : IAudioMakerStateService, IInitializable, IDisposable
    {
        private readonly IAudioFacade _audioFacade;
        private readonly Dictionary<IAudioMaker, AudioMakerStateController> _controllers = new();
        private readonly CompositeDisposable _lifecycleDisposables = new();

        public AudioMakerStateService(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
        }

        public void Initialize()
        {
            // при необходимости подписаться на удаление maker'ов, чтобы убрать контроллеры
        }

        public void EnterState(IAudioMaker maker, AudioEventAsset eventAsset, AudioParameterAsset parameterAsset, float fadeDuration, bool isLoop)
        {
            if (maker == null || eventAsset == null) 
                return;
            var controller = GetOrCreateController(maker);
            controller.EnterState(eventAsset, parameterAsset, fadeDuration, isLoop);
        }

        public void ExitState(IAudioMaker maker, AudioEventAsset eventAsset, float fadeDuration, bool isLoop)
        {
            if (maker == null || eventAsset == null) 
                return;
            if (_controllers.TryGetValue(maker, out var controller))
                controller.ExitState(eventAsset, fadeDuration, isLoop);
        }

        private AudioMakerStateController GetOrCreateController(IAudioMaker maker)
        {
            if (!_controllers.TryGetValue(maker, out var controller))
            {
                controller = new AudioMakerStateController(maker, _audioFacade);
                _controllers[maker] = controller;

                // Автоматическое удаление контроллера при уничтожении GameObject
                var go = maker.AudioMakerObject;
                if (go != null)
                {
                    go.OnDestroyAsObservable()
                        .Subscribe(_ =>
                        {
                            if (_controllers.TryGetValue(maker, out var c))
                            {
                                c.Dispose();
                                _controllers.Remove(maker);
                            }
                        })
                        .AddTo(_lifecycleDisposables);
                }

                // TODO: Так же надо сделать жизненый цикл по самому мейкеру
                // - если его убили, что б он перестал издавать звуки если находится в коллайдере
                // и что бы это не было зависимо от его скорости
                // Подготовить удаление из словаря при неактивности объекта
            }
            return controller;
        }

        public void Dispose()
        {
            foreach (var ctl in _controllers.Values)
                ctl.Dispose();
            _controllers.Clear();
            _lifecycleDisposables.Dispose();
        }
    }
}
