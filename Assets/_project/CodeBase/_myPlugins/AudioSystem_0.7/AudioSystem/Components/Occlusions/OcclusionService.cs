using AudioSystem.Components.Interfaces.Components;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components.Occlusions
{
    public class OcclusionService : IOcclusionService, IInitializable, IDisposable
    {
        private readonly IAudioService _audioService;
        private readonly HashSet<IOcclusionEmitter> _emitters = new();
        private readonly Transform _listenerTransform;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
        private IDisposable _updateSubscription;

        public OcclusionService(IAudioService audioService)
        {
            //TODO: , Camera camera прокидывать через DI
            _audioService = audioService;
            _listenerTransform = Camera.main.transform;
        }

        public void Initialize()
        {
            if (_listenerTransform == null)
                Debug.LogWarning("[OcclusionService] Main camera not found, occlusion disabled.");

            _updateSubscription = Observable.EveryUpdate().Subscribe(_ => UpdateOcclusion());
        }

        private void UpdateOcclusion()
        {
            if (_listenerTransform == null || _emitters.Count == 0) 
                return;

            Vector3 listenerPos = _listenerTransform.position;

            foreach (var emitter in _emitters)
            {
                if (emitter == null || emitter.EmitterTransform == null) 
                    continue;

                Vector3 emitterPos = emitter.EmitterTransform.position;
                float distance = Vector3.Distance(listenerPos, emitterPos);
                if (distance < 0.01f) 
                    continue;

                Vector3 direction = emitterPos - listenerPos;
                int hitCount = Physics.RaycastNonAlloc(listenerPos, direction.normalized, _hitBuffer, distance);

                for (int i = 0; i < hitCount; i++)
                {
                    if (_hitBuffer[i].collider.TryGetComponent<OcclusionAudioComponent>(out var occlusion))
                    {
                        foreach (var param in occlusion.Parameters)
                        {
                            if (param.Parameter != null)
                                _audioService.SetParameter(param.Parameter, param.Value, emitter.EmitterTransform.gameObject);
                        }
                        break; // первый попавшийся блокирует звук
                    }
                }
            }
        }

        public void RegisterEmitter(IOcclusionEmitter emitter) => _emitters.Add(emitter);
        public void UnregisterEmitter(IOcclusionEmitter emitter) => _emitters.Remove(emitter);

        public void Dispose()
        {
            _updateSubscription?.Dispose();
            _emitters.Clear();
        }
    }
}
