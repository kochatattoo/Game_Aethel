using PetProject.Navigation;
using UnityEngine;

namespace CodeBase.Hero
{
    public sealed class HeroNavigationAdapter : ICharacterMovement
    {
        private readonly HeroMotor _hero;
        private readonly NavMeshGridMap _map;

        public HeroNavigationAdapter(HeroMotor hero, NavMeshGridMap map)
        {
            _hero = hero;
            _map = map;
        }

        public bool TryMove(NavigationPoint destination, out string error) =>
            _hero.MoveNormally(new Vector3(destination.X, destination.Y, destination.Z), _map, out error);

        public void Stop() => _hero.Stop();
    }
}
