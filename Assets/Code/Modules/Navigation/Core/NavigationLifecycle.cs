namespace PetProject.Navigation
{
    public enum NavigationLifecycleState { Idle, WaitingForMovement, Moving }

    public interface INavigationLifecycle
    {
        NavigationLifecycleState State { get; }
        void Begin();
        bool Update(bool moving);
        void Reset();
    }

    public sealed class NavigationLifecycle : INavigationLifecycle
    {
        public NavigationLifecycleState State { get; private set; }

        public void Begin() => State = NavigationLifecycleState.WaitingForMovement;

        public bool Update(bool moving)
        {
            if (moving)
            {
                State = NavigationLifecycleState.Moving;
                return false;
            }
            if (State != NavigationLifecycleState.Moving) return false;
            State = NavigationLifecycleState.Idle;
            return true;
        }

        public void Reset() => State = NavigationLifecycleState.Idle;
    }
}
