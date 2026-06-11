using Infrastructure.AudioSystem.Components;

namespace AudioSystem.Room
{
    public interface IIndoorRoomService
    {
        void RegisterPortal(PortalOpeningComponent portalOpeningComponent);
        void UnregisterPortal(PortalOpeningComponent portalOpeningComponent);
    }
}
