using AudioSystem.Components.Interfaces.Components;
using AudioSystem.DebugComponent;
using Infrastructure.AudioSystem.Parameters;
using Infrastructure.AudioSystem.Utils;
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
        private readonly GameObject _mainListener;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
        private IDisposable _updateSubscription;

        private readonly LayerMask _obstructionLayerMask;
        private readonly float _obstructionChangeSpeed;
        private readonly float _maxObstructionDistance;
        private readonly float _updateInterval;

        private readonly List<IOcclusionEmitter> _invalidEmitters = new List<IOcclusionEmitter>();
        private readonly Dictionary<IOcclusionEmitter, float> _smoothedObstructions = new();

#if UNITY_EDITOR
        public List<OcclusionDebugEntry> DebugEntries { get; } = new List<OcclusionDebugEntry>();
#endif

        public OcclusionService(IAudioService audioService,  AudioDatabase data)
        {
            _audioService = audioService;
            // TODO: Camera в DI
            if (Camera.main != null)
            {
                _listenerTransform = Camera.main.transform;
                _mainListener = Camera.main.gameObject;
            }
            _obstructionLayerMask = data.ObstructionLayerMask;
            _obstructionChangeSpeed = data.ObstructionChangeSpeed;
            _maxObstructionDistance = data.MaxObstructionDistance;
            _updateInterval = data.ObstructionUpdateInterval;
        }

        public void Initialize()
        {
            if (_listenerTransform == null || _mainListener == null)
            {
                Debug.LogWarning("[OcclusionService] Main camera not found, occlusion disabled.");
                return;
            }

            _updateSubscription = Observable.Interval(TimeSpan.FromSeconds(_updateInterval))
                                            .Subscribe(UpdateOcclusion);
        }

        private void UpdateOcclusion(long _)
        {
            if (_listenerTransform == null || _mainListener == null || _emitters.Count == 0)
                return;

            if (!_audioService.IsEngineInitialized())
                return;

            Vector3 listenerPos = _listenerTransform.position;
            _invalidEmitters.Clear();

#if UNITY_EDITOR
            DebugEntries.Clear(); // ИСПРАВЛЕНО: Очищаем дебаг, чтобы избежать утечки памяти
#endif

            foreach (var emitter in _emitters)
            {
                if (!IsEmitterValid(emitter))
                {
                    _invalidEmitters.Add(emitter);
                    continue;
                }

                GameObject emitterGo = emitter.EmitterTransform.gameObject;
                float distance = Vector3.Distance(listenerPos, emitterGo.transform.position);
                if (distance > _maxObstructionDistance || distance < 0.1f)
                    continue;

                float targetObstruction = CalculateTargetObstruction(listenerPos, emitterGo, distance);

                ApplySmoothedObstruction(emitter, targetObstruction);

#if UNITY_EDITOR
                // Заполняем отладку
                DebugEntries.Add(new OcclusionDebugEntry
                {
                    Emitter = emitter,
                    ListenerPos = listenerPos,
                    EmitterPos = emitterGo.transform.position,
                    ObstructionValue = _smoothedObstructions.TryGetValue(emitter, out float val) ? val : targetObstruction,
                    IsActive = true,
                    LastUpdateTime = Time.time
                });
# endif
            }

            foreach (var inv in _invalidEmitters)
                UnregisterEmitter(inv);
        }

        private static bool IsEmitterValid(IOcclusionEmitter emitter)
        {
            if (emitter == null || emitter.EmitterTransform == null)
                return false;

            GameObject go = emitter.EmitterTransform.gameObject;
            if (go == null || !go.activeInHierarchy)
                return false;

            return AudioValidator.IsGameObjectReady(go, "Occlusion");
        }

        private float CalculateTargetObstruction(Vector3 listenerPos, GameObject emitterGo, float distance)
        {
            Vector3 direction = emitterGo.transform.position - listenerPos;
            int hitCount = Physics.RaycastNonAlloc(listenerPos, direction.normalized,
                _hitBuffer, distance, _obstructionLayerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hitBuffer[i];

                // Пропускаем коллайдеры самого эмиттера
                if (hit.transform == null || hit.transform.IsChildOf(emitterGo.transform) || hit.transform == emitterGo.transform)
                    continue;

                if (IsAcousticZone(hit.collider))
                    return 0f;

                if (hit.collider.TryGetComponent<OcclusionObstructionComponent>(out var obs))
                    return obs.GetObstructionValue(hit.distance);

                if (hit.collider.TryGetComponent<OcclusionAudioComponent>(out var oldObs))
                {
                    float maxVal = 0f;
                    foreach (var p in oldObs.Parameters)
                    {
                        if (p.Parameter != null)
                            maxVal = Mathf.Max(maxVal, p.Value);
                    }
                    return maxVal;
                }
            }
            return 0f;
        }

        private static bool IsAcousticZone(Collider col)
        {
            if (col == null)
                return false;

            return col.GetComponent<AkRoom>() != null ||
                   col.GetComponent<RoomComponent>() != null ||
                   col.GetComponent<EnvironmentAudioComponent>() != null ||
                   col.GetComponent<EnvironmentPortalComponent>() != null ||
                   col.GetComponent<RoomPortalComponent>() != null;
        }

        private void ApplySmoothedObstruction(IOcclusionEmitter emitter, float targetObstruction)
        {
            if (_mainListener == null)
                return;

            if (!_smoothedObstructions.TryGetValue(emitter, out float current))
                current = 0f;

            float newObstruction = Mathf.MoveTowards(current, targetObstruction,
                _obstructionChangeSpeed * _updateInterval);
            _smoothedObstructions[emitter] = newObstruction;

            GameObject go = emitter.EmitterTransform.gameObject;
            if (go != null && go.activeInHierarchy)
            {
                _audioService.SetObjectObstructionAndOcclusion(go, _mainListener, newObstruction, newObstruction);
            }
        }

        public void RegisterEmitter(IOcclusionEmitter emitter)
        {
            if (emitter == null)
                return;

            _emitters.Add(emitter);
            if (!_smoothedObstructions.ContainsKey(emitter))
                _smoothedObstructions[emitter] = 0f;
        }

        public void UnregisterEmitter(IOcclusionEmitter emitter)
        {
            if (emitter == null || !_emitters.Contains(emitter))
                return;

            _emitters.Remove(emitter);

            if (_mainListener != null && emitter?.EmitterTransform != null)
            {
                GameObject go = emitter.EmitterTransform.gameObject;
                if (go != null && go.activeInHierarchy && AudioValidator.IsGameObjectReady(go, "UnregisterEmitter"))
                {
                    _audioService.SetObjectObstructionAndOcclusion(go, _mainListener, 0f, 0f);
                }
            }
            _smoothedObstructions.Remove(emitter);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateSubscription?.Dispose();
                if (_mainListener != null)
                {
                    foreach (var emitter in _emitters)
                    {
                        if (emitter?.EmitterTransform != null)
                        {
                            GameObject go = emitter.EmitterTransform.gameObject;
                            if (go.activeInHierarchy && AudioValidator.IsGameObjectReady(go, "Dispose"))
                            {
                                _audioService.SetObjectObstructionAndOcclusion(go, _mainListener, 0f, 0f);
                            }
                        }
                    }
                }
                _emitters.Clear();
                _invalidEmitters.Clear();
                _smoothedObstructions.Clear();
            }
        }
    }
}
