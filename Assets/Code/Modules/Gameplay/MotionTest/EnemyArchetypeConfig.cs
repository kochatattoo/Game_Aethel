using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using UnityEngine;

namespace CodeBase.MotionTest
{
    public enum EnemyArchetype { Skeleton, SkeletonArcher, SkeletonMage, SkeletonCaptain, SkeletonGeneral }
    public enum EnemyRank { Normal, Uncommon, Boss }
    public enum EnemyAttackAttribute { Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma }

    /// <summary>Настраиваемый архетип противника: ранг, AI, характеристики и кубики.</summary>
    [CreateAssetMenu(fileName = "EnemyArchetypeConfig", menuName = "PetProject/Enemies/Archetype")]
    public sealed class EnemyArchetypeConfig : ScriptableObject
    {
        [SerializeField] private EnemyArchetype _archetype;
        [SerializeField] private EnemyRank _rank;
        [SerializeField] private string _displayName = "Скелет";
        [SerializeField] private GameObject _prefab;
        [SerializeField] private MeleeTacticsAIConfig _ai;
        [SerializeField, Range(1, 30)] private int _strength = 10;
        [SerializeField, Range(1, 30)] private int _dexterity = 10;
        [SerializeField, Range(1, 30)] private int _constitution = 10;
        [SerializeField, Range(1, 30)] private int _intelligence = 10;
        [SerializeField, Range(1, 30)] private int _wisdom = 10;
        [SerializeField, Range(1, 30)] private int _charisma = 10;
        [SerializeField] private EnemyAttackAttribute _attackAttribute;
        [SerializeField, Min(1)] private int _baseMaxHealth = 5;
        [SerializeField, Min(0)] private int _healthPerConstitutionModifier = 1;
        [SerializeField, Min(0)] private int _proficiencyBonus = 2;
        [SerializeField, Min(0)] private int _baseDefense = 10;
        [SerializeField, Min(0)] private int _armorBonus = 2;
        [SerializeField, Min(2)] private int _attackDiceSides = 20;
        [SerializeField, Min(1)] private int _damageDiceCount = 1;
        [SerializeField, Min(2)] private int _damageDiceSides = 4;
        [SerializeField] private int _damageBonus;

        public EnemyArchetype Archetype => _archetype;
        public EnemyRank Rank => _rank;
        public string DisplayName => _displayName;
        public GameObject Prefab => _prefab;
        public MeleeTacticsAIConfig AI => _ai;
        public int Strength => _strength;
        public int Dexterity => _dexterity;
        public int Constitution => _constitution;
        public int Intelligence => _intelligence;
        public int Wisdom => _wisdom;
        public int Charisma => _charisma;
        public int InitiativeBonus => Modifier(_dexterity);
        public int AttackBonus => AttackModifier + _proficiencyBonus;
        public int Defense => _baseDefense + _armorBonus + Modifier(_dexterity);
        public int MaxHealth => Mathf.Max(1, _baseMaxHealth + Modifier(_constitution) * _healthPerConstitutionModifier);
        public int AttackDiceSides => _attackDiceSides;
        public int DamageDiceCount => _damageDiceCount;
        public int DamageDiceSides => _damageDiceSides;
        public int DamageBonus => _damageBonus;
        public Color HealthBarColor => _rank == EnemyRank.Boss ? new Color(.75f, .2f, .85f) :
            _rank == EnemyRank.Uncommon ? new Color(1f, .65f, .15f) : new Color(.9f, .22f, .18f);

        private int AttackModifier => _attackAttribute switch
        {
            EnemyAttackAttribute.Strength => Modifier(_strength),
            EnemyAttackAttribute.Dexterity => Modifier(_dexterity),
            EnemyAttackAttribute.Constitution => Modifier(_constitution),
            EnemyAttackAttribute.Intelligence => Modifier(_intelligence),
            EnemyAttackAttribute.Wisdom => Modifier(_wisdom),
            EnemyAttackAttribute.Charisma => Modifier(_charisma),
            _ => 0
        };

        public static int Modifier(int characteristic) => Mathf.FloorToInt((characteristic - 10) * .5f);

        private void OnValidate()
        {
            _strength = Mathf.Clamp(_strength, 1, 30);
            _dexterity = Mathf.Clamp(_dexterity, 1, 30);
            _constitution = Mathf.Clamp(_constitution, 1, 30);
            _intelligence = Mathf.Clamp(_intelligence, 1, 30);
            _wisdom = Mathf.Clamp(_wisdom, 1, 30);
            _charisma = Mathf.Clamp(_charisma, 1, 30);
            _baseMaxHealth = Mathf.Max(1, _baseMaxHealth);
            _healthPerConstitutionModifier = Mathf.Max(0, _healthPerConstitutionModifier);
            _proficiencyBonus = Mathf.Max(0, _proficiencyBonus);
            _baseDefense = Mathf.Max(0, _baseDefense);
            _armorBonus = Mathf.Max(0, _armorBonus);
            _attackDiceSides = Mathf.Max(2, _attackDiceSides);
            _damageDiceCount = Mathf.Max(1, _damageDiceCount);
            _damageDiceSides = Mathf.Max(2, _damageDiceSides);
        }
    }
}
