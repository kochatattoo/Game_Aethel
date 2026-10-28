using System;
using Code.Hero.Attributes;
using UniRx;
using UnityEngine;
namespace Code.Hero.Data
{
    public sealed class HeroDerivedStats : IDisposable
    {
        private readonly CompositeDisposable _lifetime=new();
        private readonly ReactiveProperty<float> _health=new(),_damage=new();
        public IReadOnlyReactiveProperty<float> MaxHealth=>_health;
        public IReadOnlyReactiveProperty<float> Damage=>_damage;
        public HeroDerivedStats(HeroClass hero,CharacterBalance balance)
        {
            hero.Attributes[AttributeConstants.STM].Subscribe(v=>_health.Value=Mathf.Max(1,balance.BaseHealth+(v-hero.ReferenceStamina)*balance.HealthPerStamina)).AddTo(_lifetime);
            hero.Attributes[AttributeConstants.STR].Subscribe(v=>_damage.Value=Mathf.Max(0,balance.BaseDamage+(v-hero.ReferenceStrength)*balance.DamagePerStrength)).AddTo(_lifetime);
        }
        public void Dispose(){_lifetime.Dispose();_health.Dispose();_damage.Dispose();}
    }
}

