using UnityEngine;

namespace VFXSystem.Resolver
{
    public class VFXCompositeValidator<T> : IVFXValidator<T>
    {
        private readonly IVFXValidator<T>[] _validators;
        private readonly int _length;

        public VFXCompositeValidator( VFXCooldownValidator<T> cooldown,
                                      VFXCountValidator<T> count,
                                      VFXDistanceValidator<T> distance,
                                      VFXFrustumValidator<T> frustum,
                                      VFXPerformanceValidator<T> performance)
        {
            _validators = new IVFXValidator<T>[]
            {
                cooldown,    // 1. Самая быстрая проверка (время)
                count,       // 2. Простая арифметика
                distance,    // 3. Математика векторов
                frustum,     // 4. Проверка видимости (тяжелее)
                performance  // 5. Системные показатели (самая сложная)
            };

            _length = _validators.Length;
        }

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            for (int i = 0; i < _length; i++)
            {
                if (!_validators[i].CanSpawn(context))
                {
                    Debug.Log($"Validators: {_validators[i]} cann't spawn VFX");
                    return false; 
                }
            }
            return true;
        }

        public void OnEffectSpawned(T definition)
        {
            for (int i = 0; i < _length; i++)
            {
                _validators[i].OnEffectSpawned(definition);
            }
        }

        public void OnEffectDespawned(T definition)
        {
            for (int i = 0; i < _length; i++)
            {
                _validators[i].OnEffectDespawned(definition);
            }
        }
    }
}
