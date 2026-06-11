namespace AudioSystem.Components.Interfaces.Components
{
    public interface IRoomComponent
    {
        ulong RoomID { get; }
        void SetRoomId(ulong id);
        void RegisterRoom();
        void UnregisterRoom();
    }
}
