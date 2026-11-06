using Infrastructure.AudioSystem.Components;
using UnityEngine;

namespace AudioSystem.Components.Interfaces.Components
{
    public interface IPortalOpening
    {
        bool IsOpen { get; }
        Vector3 Position { get; }
        IndoorRoomComponent Room { get; }
    }
}
