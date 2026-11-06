using Infrastructure.AudioSystem.Components;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Infrastructure.AudioSystem.Zones
{
    public class AudioZoneRegistry : IAudioZoneRegistry, IDisposable
    {
        private readonly List<EnvironmentAudioComponent> _environments = new();
        private readonly List<RoomComponent> _rooms = new();
        private readonly List<IndoorRoomComponent> _indoorRooms = new();
        private readonly BehaviorSubject<bool> _readySubject = new(false);

        public IObservable<bool> Ready => _readySubject.AsObservable();
        public bool IsReady => _readySubject.Value;
        public IReadOnlyList<EnvironmentAudioComponent> Environments => _environments;
        public IReadOnlyList<RoomComponent> Rooms => _rooms;
        public IReadOnlyList<IndoorRoomComponent> IndoorRooms => _indoorRooms;
        public EventWrapper<IndoorRoomComponent> OnIndoorRoomAdded { get; } = new();
        public EventWrapper<IndoorRoomComponent> OnIndoorRoomRemoved { get; } = new();

        public void MarkReady() => _readySubject.OnNext(true);

        public void RegisterEnvironment(EnvironmentAudioComponent environment)
        {
            if (environment != null && !_environments.Contains(environment))
                _environments.Add(environment);
        }

        public void UnregisterEnvironment(EnvironmentAudioComponent environment)
        {
            if(_environments.Contains(environment))
                _environments.Remove(environment);
            else
                Debug.LogError("Environment doesn't contains in List");
        }

        public void RegisterRoom(RoomComponent room)
        {
            if (room != null && !_rooms.Contains(room))
                _rooms.Add(room);
        }

        public void UnregisterRoom(RoomComponent room)
        {
            if (_rooms.Contains(room))
                _rooms.Remove(room);
            else
                Debug.LogError("Room doesn't contains in List");
        }

        public void RegisterIndoorRoom(IndoorRoomComponent room)
        {
            if (room != null && !_indoorRooms.Contains(room))
            {
                _indoorRooms.Add(room);
                OnIndoorRoomAdded.Publish(room);
            }
        }

        public void UnregisterIndoorRoom(IndoorRoomComponent room)
        {
            if (_indoorRooms.Contains(room))
            {
                _indoorRooms.Remove(room);
                OnIndoorRoomRemoved.Publish(room);
            }
            else
                Debug.LogError("Room doesn't contains in List");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _environments.Clear();
                _rooms.Clear();
            }
        }
    }
}
