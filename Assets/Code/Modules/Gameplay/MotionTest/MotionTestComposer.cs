using PetProject.CharacterStats;
using PetProject.EnemyArchetypes;
using PetProject.Navigation;

namespace CodeBase.MotionTest
{
    public interface IMotionTestComposition
    {
        IReadOnlyCombatStats HeroStats { get; }
        IEnemyArchetype GetEnemyArchetype(SkeletonEnemySpawn spawn);
        INavigationLifecycle CreateNavigationLifecycle();
        ICharacterMovement CreateCharacterMovement(MotionHero hero, SurfaceGridMap map);
    }

    public sealed class MotionTestComposer : IMotionTestComposition
    {
        private readonly MotionCombatRulesConfig _rules;

        public IReadOnlyCombatStats HeroStats => _rules != null ? _rules.HeroStats : null;

        public MotionTestComposer(MotionCombatRulesConfig rules)
        {
            _rules = rules;
        }

        public IEnemyArchetype GetEnemyArchetype(SkeletonEnemySpawn spawn) => spawn?.Archetype?.Definition;

        public INavigationLifecycle CreateNavigationLifecycle() => new NavigationLifecycle();

        public ICharacterMovement CreateCharacterMovement(MotionHero hero, SurfaceGridMap map) =>
            new MotionCharacterMovementAdapter(hero, map);
    }
}
