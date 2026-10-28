using System;
namespace Code.Hero.Data
{
    [Serializable]
    public sealed class CharacterBalance
    {
        public float BaseHealth=50;
        public float HealthPerStamina=5;
        public float BaseDamage=2;
        public float DamagePerStrength=0.5f;
    }
}

