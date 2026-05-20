namespace AudioSystem.Components.Interfaces.Components
{
    public interface IRoomComponent
    {
        uint RoomID { get; }
        void RegisterRoom();
        void UnregisterRoom();
    }
}
