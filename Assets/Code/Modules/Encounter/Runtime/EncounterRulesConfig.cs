using PetProject.CharacterStats;
using UnityEngine;

namespace PetProject.Encounter
{
    /// <summary>Общие правила столкновения и характеристики управляемого героя.</summary>
    [CreateAssetMenu(fileName = "EncounterRulesConfig", menuName = "PetProject/Encounter/Combat Rules")]
    public sealed class EncounterRulesConfig : ScriptableObject, IEncounterRules
    {
        [SerializeField, Min(1)] private int _actionsPerTurn = 6;
        [SerializeField, Min(1)] private int _attackCost = 3;
        [SerializeField, Min(0)] private float _groupLinkRadius = 12;
        [SerializeField, Min(.5f)] private float _enemyDecisionDelay = .75f;
        [SerializeField, Range(1, 30)] private int _heroStrength = 12;
        [SerializeField, Range(1, 30)] private int _heroDexterity = 12;
        [SerializeField, Range(1, 30)] private int _heroConstitution = 12;
        [SerializeField, Range(1, 30)] private int _heroIntelligence = 10;
        [SerializeField, Range(1, 30)] private int _heroWisdom = 10;
        [SerializeField, Range(1, 30)] private int _heroCharisma = 10;
        [SerializeField, Min(1)] private int _heroBaseMaxHealth = 10;
        [SerializeField, Min(0)] private int _heroHealthPerConstitutionModifier = 2;
        [SerializeField, Min(0)] private int _heroProficiencyBonus = 2;
        [SerializeField, Min(0)] private int _heroBaseDefense = 10;
        [SerializeField, Min(0)] private int _heroArmorBonus = 1;
        [SerializeField, Min(1)] private int _heroDamage = 4;
        [System.NonSerialized] private IReadOnlyCombatStats _heroStatsOverride;

        public int ActionsPerTurn => _actionsPerTurn;
        public int AttackCost => _attackCost;
        public float GroupLinkRadius => _groupLinkRadius;
        public float EnemyDecisionDelay => _enemyDecisionDelay;
        public int HeroStrength => _heroStrength;
        public int HeroDexterity => _heroDexterity;
        public int HeroConstitution => _heroConstitution;
        public int HeroIntelligence => _heroIntelligence;
        public int HeroWisdom => _heroWisdom;
        public int HeroCharisma => _heroCharisma;
        public int HeroMaxHealth => HeroStats.MaxHealth;
        public int HeroDefense => HeroStats.Defense;
        public int HeroAttackBonus => HeroStats.AttackBonus;
        public int HeroInitiativeBonus => HeroStats.InitiativeBonus;
        public int HeroDamage => _heroDamage;
        public IReadOnlyCombatStats HeroStats => _heroStatsOverride ?? CombatStatsCalculator.Calculate(
            new CharacterAttributes(_heroStrength, _heroDexterity, _heroConstitution, _heroIntelligence,
                _heroWisdom, _heroCharisma),
            new CombatStatsSettings(_heroBaseMaxHealth, _heroHealthPerConstitutionModifier,
                _heroProficiencyBonus, _heroBaseDefense, _heroArmorBonus, CharacteristicType.Strength));

        public void SetHeroStats(int maxHealth, int damage)
        {
            var defaults = HeroStats;
            _heroStatsOverride = new CombatStats(maxHealth, defaults.InitiativeBonus, defaults.AttackBonus,
                defaults.Defense);
            _heroDamage = damage;
        }

        public static int Modifier(int characteristic) => CharacterAttributes.Modifier(characteristic);

        private void OnValidate()
        {
            _actionsPerTurn = Mathf.Max(1, _actionsPerTurn);
            _attackCost = Mathf.Clamp(_attackCost, 1, _actionsPerTurn);
            _groupLinkRadius = Mathf.Max(0, _groupLinkRadius);
            _enemyDecisionDelay = Mathf.Max(.5f, _enemyDecisionDelay);
            _heroStrength = Mathf.Clamp(_heroStrength, 1, 30);
            _heroDexterity = Mathf.Clamp(_heroDexterity, 1, 30);
            _heroConstitution = Mathf.Clamp(_heroConstitution, 1, 30);
            _heroIntelligence = Mathf.Clamp(_heroIntelligence, 1, 30);
            _heroWisdom = Mathf.Clamp(_heroWisdom, 1, 30);
            _heroCharisma = Mathf.Clamp(_heroCharisma, 1, 30);
            _heroBaseMaxHealth = Mathf.Max(1, _heroBaseMaxHealth);
            _heroHealthPerConstitutionModifier = Mathf.Max(0, _heroHealthPerConstitutionModifier);
            _heroProficiencyBonus = Mathf.Max(0, _heroProficiencyBonus);
            _heroBaseDefense = Mathf.Max(0, _heroBaseDefense);
            _heroArmorBonus = Mathf.Max(0, _heroArmorBonus);
            _heroDamage = Mathf.Max(1, _heroDamage);
        }
    }
}
