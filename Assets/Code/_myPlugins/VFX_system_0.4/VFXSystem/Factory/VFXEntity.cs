using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VFXSystem.Parameters.Definition;
using VFXSystem.Resolver;
using VFXSystem.VFXTypes;
using Zenject;

namespace VFXSystem.Factory
{
    /// <summary>
    /// Главная сущность визуального эффекта в системе пулинга.
    /// Комбинирует частицы, декали и доп. эффекты на основе HitVFXDefinition 
    /// и управляет их общим временем жизни.
    /// </summary>
    public class VFXEntity: MonoBehaviour, IPoolable<HitVFXDefinition, IMemoryPool>, IDisposable
    {
        [SerializeField] 
        private Transform _particleContainer;
        [SerializeField] 
        private Transform _decalContainer;
        [SerializeField] 
        private Transform _additionalContainer;

        private IMemoryPool _pool;
        private IVFXValidator<HitVFXDefinition> _validator;
        private HitVFXDefinition _currentDefinition;
        private CancellationTokenSource _cts;

        private readonly Dictionary<ISubVFX, ISubVFX> _instantiatedPrefabs = new();
        private readonly List<ISubVFX> _activeEffects = new();

        [Inject]
        private void Construct(IVFXValidator<HitVFXDefinition> validator)
        {
            _validator = validator;
        }

        public void OnSpawned(HitVFXDefinition definition, IMemoryPool pool)
        {
            _pool = pool;
            _currentDefinition = definition;
            _cts = new CancellationTokenSource();

            _validator.OnEffectSpawned(definition);

            PrepareEffect(_currentDefinition.ParticlePrefab, _particleContainer);
            PrepareEffect(_currentDefinition.DecalPrefab, _decalContainer);
            PrepareEffect(_currentDefinition.AdditionalEffectPrefab, _additionalContainer);
        }

        /// <summary>
        /// Активирует все эффекты в заданной точке.
        /// </summary>
        public void PlayAt(Vector3 position, Quaternion rotation, Transform target = null, float impactScale = 1f)
        {
            transform.SetParent(target);
            transform.localScale = Vector3.one;
            transform.SetPositionAndRotation(position, rotation);

            // Запускаем асинхронный жизненный цикл
            PlayAndLifecycleAsync(target, impactScale, _cts.Token).Forget();
        }

        public void OnDespawned()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            NotifyValidatorRemoval();

            foreach (var effect in _activeEffects)
            {
                if (effect != null && !ReferenceEquals(effect, null))
                {
                    effect.Stop();
                }
            }
            foreach (var instance in _instantiatedPrefabs.Values)
            {
                instance?.GameObject.SetActive(false);
            }

            _activeEffects.Clear();
            _pool = null;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnDestroy()
        {
            NotifyValidatorRemoval();
            Dispose();
        }


        [CanBeNull]
        private void PrepareEffect(ISubVFX prefab, Transform container)
        {
            if (prefab == null || prefab.GameObject == null) 
                return;

            if (!_instantiatedPrefabs.TryGetValue(prefab, out var instance))
            {
                var go = Instantiate(prefab.GameObject, container);
                instance = go.GetComponent<ISubVFX>();
                _instantiatedPrefabs.Add(prefab, instance);
            }

            instance.GameObject.SetActive(true);
            instance.GameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _activeEffects.Add(instance);
        }
        private async UniTaskVoid PlayAndLifecycleAsync(Transform target, float impactScale, CancellationToken token)
        {
            float maxDuration = 0;

            for (int i = 0; i < _activeEffects.Count; i++)
            {
                var effect = _activeEffects[i];
                effect.Play(impactScale);

                if (effect.Duration > maxDuration)
                    maxDuration = effect.Duration;

                if (_activeEffects.Count > 1 && i < _activeEffects.Count - 1)
                {
                    // Используем PreUpdate, чтобы эффект визуально появился как можно раньше в следующем кадре
                    await UniTask.Yield(PlayerLoopTiming.PreUpdate, token);
                }
            }
    
            var followTask = FollowTargetAsync(target, token);

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(maxDuration), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            DespawnVFX();
        }

        private async UniTask FollowTargetAsync(Transform target, CancellationToken token)
        {
            if (target == null)
                return;

            while (target != null && target.gameObject.activeInHierarchy)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (transform != null)
                transform.SetParent(null);
        }

        private void NotifyValidatorRemoval()
        {
            if (_currentDefinition != null && _validator != null)
            {
                _validator.OnEffectDespawned(_currentDefinition);
                _currentDefinition = null; 
            }
        }

        private void DespawnVFX()
        {
            transform.SetParent(null);
            _pool.Despawn(this);
        }

        public class Pool: MonoPoolableMemoryPool<HitVFXDefinition, IMemoryPool, VFXEntity> { }
    }
}
