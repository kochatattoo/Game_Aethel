using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using VFXSystem.Parameters.Definition;
using VFXSystem.VFXTypes;
using Zenject;

namespace VFXSystem.Factory
{
    /// <summary>
    /// Главная сущность визуального эффекта в системе пулинга.
    /// Комбинирует частицы, декали и доп. эффекты на основе HitVFXDefinition 
    /// и управляет их общим временем жизни.
    /// </summary>
    public class VFXEntity: MonoBehaviour, IPoolable<HitVFXDefinition, IMemoryPool>
    {
        [SerializeField] 
        private Transform _particleContainer;
        [SerializeField] 
        private Transform _decalContainer;
        [SerializeField] 
        private Transform _additionalContainer;

        private IMemoryPool _pool;
        private HitVFXDefinition _currentDefinition;

        // Храним ссылки на созданные объекты, чтобы не спавнить их каждый раз (кэширование)
        private readonly Dictionary<ISubVFX, GameObject> _instantiatedPrefabs = new();
        // Список интерфейсов текущих активных объектов для управления
        private readonly List<ISubVFX> _activeEffects = new();

        public void OnSpawned(HitVFXDefinition definition, IMemoryPool pool)
        {
            _pool = pool;
            _currentDefinition = definition;
            _activeEffects.Clear();

            PrepareEffect(_currentDefinition.ParticlePrefab, _particleContainer);
            PrepareEffect(_currentDefinition.DecalPrefab, _decalContainer);
            PrepareEffect(_currentDefinition.AdditionalEffectPrefab, _additionalContainer);
        }

        /// <summary>
        /// Активирует все эффекты в заданной точке.
        /// </summary>
        public void PlayAt(Vector3 position, Quaternion rotation, float impactScale = 1f)
        {
            transform.localScale = Vector3.one;
            transform.SetPositionAndRotation(position, rotation);

            float maxDuration = 0;

            foreach (var effect in _activeEffects)
            {
                // Вот тут можем передавать данные для наших различных VFX (можно подумать как настроить тексутуру)
                effect.Play(impactScale); // Пока передаю только значение scale - Далее подумать над трансформом и объектом

                if (effect.Duration > maxDuration) 
                    maxDuration = effect.Duration;
            }

            if (maxDuration > 0)
            {
                Invoke(nameof(DespawnVFX), maxDuration);
            }
        }

        public void OnDespawned()
        {
            CancelInvoke();

            foreach (var effect in _activeEffects)
            {
                effect.Stop();
            }

            foreach (var instance in _instantiatedPrefabs.Values)
            {
                if (instance != null) 
                    instance.SetActive(false);
            }

            _activeEffects.Clear();
            _currentDefinition = null;
            _pool = null;
        }

        public void DespawnVFX() => _pool?.Despawn(this);

        [CanBeNull]
        private GameObject PrepareEffect(ISubVFX prefab, Transform container)
        {
            if (prefab == null) 
                return null;

            if (!_instantiatedPrefabs.TryGetValue(prefab, out var instance))
            {
                instance = Instantiate(prefab.GameObject, container);
                _instantiatedPrefabs.Add(prefab, instance);
            }

            instance.SetActive(true);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            //Под вопросом, поскольку мы вытаскием интерфейс из объекта клона - так надо
            if (instance.TryGetComponent<ISubVFX>(out var subVfx))
            {
                _activeEffects.Add(subVfx);
            }

            return instance;
        }

        public class Pool: MonoMemoryPool<HitVFXDefinition, VFXEntity> { }
    }
}
