namespace PetProject.Encounter
{
    public interface IEncounterRules
    {
        int ActionsPerTurn { get; }
        int AttackCost { get; }
        float GroupLinkRadius { get; }
        float EnemyDecisionDelay { get; }
        int HeroStrength { get; }
        int HeroDexterity { get; }
        int HeroConstitution { get; }
        int HeroIntelligence { get; }
        int HeroWisdom { get; }
        int HeroCharisma { get; }
        int HeroMaxHealth { get; }
        int HeroDefense { get; }
        int HeroAttackBonus { get; }
        int HeroInitiativeBonus { get; }
        int HeroDamage { get; }
    }
}
