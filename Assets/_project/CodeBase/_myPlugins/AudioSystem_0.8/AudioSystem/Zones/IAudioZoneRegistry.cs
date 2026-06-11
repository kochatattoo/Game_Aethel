using Infrastructure.AudioSystem.Components;
using Shared.Utils;
using System;
using System.Collections.Generic;

namespace Infrastructure.AudioSystem.Zones
{
    public interface IAudioZoneRegistry
    {
        void RegisterEnvironment(EnvironmentAudioComponent environment);
        void UnregisterEnvironment(EnvironmentAudioComponent environment);
        void RegisterRoom(RoomComponent room);
        void UnregisterRoom(RoomComponent room);
        void RegisterIndoorRoom(IndoorRoomComponent room);
        void UnregisterIndoorRoom(IndoorRoomComponent room);
        void MarkReady();

        IReadOnlyList<EnvironmentAudioComponent> Environments { get; }
        IReadOnlyList<RoomComponent> Rooms { get; }
        IReadOnlyList<IndoorRoomComponent> IndoorRooms { get; }

        bool IsReady { get; }
        IObservable<bool> Ready { get; }
        EventWrapper<IndoorRoomComponent> OnIndoorRoomAdded { get; }
        EventWrapper<IndoorRoomComponent> OnIndoorRoomRemoved { get; }
    }
}
