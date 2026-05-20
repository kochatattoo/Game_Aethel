namespace AudioSystem.Components.Interfaces.Components
{
    public interface IRoomAware
    {
        void EnterRoom(IRoomComponent room);
        void ExitRoom(IRoomComponent room);
    }
}
