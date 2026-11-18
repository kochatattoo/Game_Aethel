namespace PetProject.Navigation
{
    public interface ICharacterMovement
    {
        bool TryMove(NavigationPoint destination, out string error);
        void Stop();
    }
}
