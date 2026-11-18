using UnityEngine;

namespace CodeBase.MotionTest
{
    /// <summary>Общие правила столкновения и характеристики героя тестовой сцены.</summary>
    [CreateAssetMenu(fileName = "MotionCombatRulesConfig", menuName = "PetProject/Tests/Combat Rules")]
    public sealed class MotionCombatRulesConfig : ScriptableObject
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
        public int HeroMaxHealth => _heroBaseMaxHealth + Modifier(_heroConstitution) * _heroHealthPerConstitutionModifier;
        public int HeroDefense => _heroBaseDefense + _heroArmorBonus + Modifier(_heroDexterity);
        public int HeroAttackBonus => Modifier(_heroStrength) + _heroProficiencyBonus;
        public int HeroInitiativeBonus => Modifier(_heroDexterity);
        public int HeroDamage => _heroDamage;

        public static int Modifier(int characteristic) => Mathf.FloorToInt((characteristic - 10) * .5f);

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
