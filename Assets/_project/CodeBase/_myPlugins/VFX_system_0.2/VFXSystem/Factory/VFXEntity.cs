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
        private readonly Dictionary<ISubVFX, ISubVFX> _instantiatedPrefabs = new();
        // Список интерфейсов текущих активных объектов для управления
        private readonly List<ISubVFX> _activeEffects = new();

        private void Update()
        {
            // Если родитель (враг) внезапно исчез (был уничтожен), 
            // но эффект еще должен жить — отцепляемся и доживаем свое время
            if (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)
            {
                transform.SetParent(null);
            }
        }

        public void OnSpawned(HitVFXDefinition definition, IMemoryPool pool)
        {
            Debug.Log("Spawn");

            _pool = pool;
            _currentDefinition = definition;
            Debug.Log($"Текущее определение и пул объектов: {_currentDefinition}, {_pool}");
            _activeEffects.Clear();
            Debug.Log($"Очистка списка активных эффектов");

            PrepareEffect(_currentDefinition.ParticlePrefab, _particleContainer);
            PrepareEffect(_currentDefinition.DecalPrefab, _decalContainer);
            PrepareEffect(_currentDefinition.AdditionalEffectPrefab, _additionalContainer);
        }

        /// <summary>
        /// Активирует все эффекты в заданной точке.
        /// </summary>
        public void PlayAt(Vector3 position, Quaternion rotation, Transform target = null, float impactScale = 1f)
        {
            Debug.Log("PlayAt VFXEntity");
            transform.SetParent(target);

            transform.localScale = Vector3.one;
            transform.SetPositionAndRotation(position, rotation);

            float maxDuration = 0;

            Debug.Log($"Цикл активных эффектов: {_activeEffects}");
            foreach (var effect in _activeEffects)
            {
                Debug.Log("Проходим по активным эффектам");

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

            _currentDefinition = null;
            _pool = null;
        }

        public void DespawnVFX()
        {
            if (_pool == null) return;

            // 1. Отвязываем от врага, чтобы не удалиться вместе с ним
            transform.SetParent(null);

            // 2. Сбрасываем масштаб (важно для пула, чтобы следующий спавн не был гигантским)
            transform.localScale = Vector3.one;

            // 3. Возвращаем в пул (это вызовет OnDespawned)
            _pool.Despawn(this);
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

        public class Pool: MonoPoolableMemoryPool<HitVFXDefinition, IMemoryPool, VFXEntity> { }
    }
}
