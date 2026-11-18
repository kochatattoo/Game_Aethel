using PetProject.Navigation;
using UnityEngine;

namespace CodeBase.MotionTest
{
    internal sealed class MotionCharacterMovementAdapter : ICharacterMovement
    {
        private readonly MotionHero _hero;
        private readonly SurfaceGridMap _map;

        public MotionCharacterMovementAdapter(MotionHero hero, SurfaceGridMap map)
        {
            _hero = hero;
            _map = map;
        }

        public bool TryMove(NavigationPoint destination, out string error) =>
            _hero.MoveNormally(new Vector3(destination.X, destination.Y, destination.Z), _map, out error);

        public void Stop() => _hero.Stop();
    }
}
