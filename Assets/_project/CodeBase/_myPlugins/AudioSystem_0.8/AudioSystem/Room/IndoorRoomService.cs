using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Components;
using Infrastructure.AudioSystem.Factory;
using Infrastructure.AudioSystem.Parameters;
using Infrastructure.AudioSystem.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace AudioSystem.Room
{
    public class IndoorRoomService : IIndoorRoomService, IInitializable, IDisposable
    {
        private readonly Transform _listener;
        private readonly IAudioZoneRegistry _zoneRegistry;
        private readonly IAudioFacade _audioFacade;
        private readonly float _updateInterval;
        private readonly float _maxDistance;
        private readonly CompositeDisposable _disposables = new();

        private readonly Dictionary<IndoorRoomComponent, RoomState> _roomStates = new();
        private readonly List<IPortalOpening> _portals = new();

        private class RoomState
        {
            public readonly List<IOcclusionEmitter> Emitters = new();
            public readonly Dictionary<IOcclusionEmitter, Vector3> OriginalPositions = new();
            public bool ListenerInside;
        }

        public IndoorRoomService(IAudioZoneRegistry zoneRegistry,
            IAudioFacade audioFacade, AudioDatabase database)
        {
            _listener = Camera.main.transform;
            _zoneRegistry = zoneRegistry;
            _audioFacade = audioFacade;
            _updateInterval = database.IndoorUpdateInterval;
            _maxDistance = database.MaxIndoorRoomDistance;
        }

        public void Initialize()
        {
            if (_zoneRegistry.IsReady)
                StartUpdating();
            else
                _zoneRegistry.Ready
                    .Where(ready => ready)
                    .Take(1)
                    .Subscribe(OnReady)
                    .AddTo(_disposables);
        }

        private void OnReady(bool _) => StartUpdating();

        private void StartUpdating()
        {
            // Подписка на существующие и новые комнаты
            foreach (var room in _zoneRegistry.IndoorRooms)
                RegisterRoom(room);

            _zoneRegistry.OnIndoorRoomAdded.Subscribe(RegisterRoom).AddTo(_disposables);
            _zoneRegistry.OnIndoorRoomRemoved.Subscribe(UnregisterRoom).AddTo(_disposables);

            Observable.Interval(TimeSpan.FromSeconds(_updateInterval))
                .Subscribe(UpdateRooms).AddTo(_disposables);
        }

        private void RegisterRoom(IndoorRoomComponent room)
        {
            if (_roomStates.ContainsKey(room))
                return;

            var state = new RoomState();
            _roomStates[room] = state;

            bool listenerInside = room.RoomCollider.bounds.Contains(_listener.position);
            state.ListenerInside = listenerInside;

            room.OnEmitterEntered.Subscribe(Emitterentered(room, state));
            room.OnEmitterExited.Subscribe(EmitterExited(room, state));
            room.OnListenerEntered.Subscribe(ListenerEntered(room, state));
            room.OnListenerExited.Subscribe(ListenerExited(room, state));
        }

        private Action<IOcclusionEmitter> Emitterentered(IndoorRoomComponent room, RoomState state) =>
            emitter => OnEmitterEntered(room, state, emitter);

        private Action<IOcclusionEmitter> EmitterExited(IndoorRoomComponent room, RoomState state) =>
            emitter => OnEmitterExited(room, state, emitter);

        private Action<IndoorRoomListener> ListenerEntered(IndoorRoomComponent room, RoomState state) =>
            _ => OnListenerEntered(room, state);

        private Action<IndoorRoomListener> ListenerExited(IndoorRoomComponent room, RoomState state) =>
            _ => OnListenerExited(room, state);

        private void OnEmitterEntered(IndoorRoomComponent room, RoomState state, IOcclusionEmitter emitter)
        {
            state.Emitters.Add(emitter);
            state.OriginalPositions[emitter] = emitter.EmitterTransform.position;

            if (!state.ListenerInside)
            {
                var portal = GetPortalForRoom(room);
                if (portal != null && portal.IsOpen && emitter is IOcclusionEmitter pos)
                    pos.OverridePosition(portal.Position);
                else if (portal == null || !portal.IsOpen)
                {
                    if (emitter is BaseAudioEventComponent baseComp)
                        baseComp.Stop();
                }
            }
        }

        private void OnEmitterExited(IndoorRoomComponent room, RoomState state, IOcclusionEmitter emitter)
        {
            state.Emitters.Remove(emitter);
            state.OriginalPositions.Remove(emitter);
            if (emitter is IOcclusionEmitter pos)
                pos.RestoreOriginalPosition();
        }

        private void OnListenerEntered(IndoorRoomComponent room, RoomState state)
        {
            state.ListenerInside = true;
            foreach (var emitter in state.Emitters)
            {
                if (emitter is IOcclusionEmitter pos)
                    pos.RestoreOriginalPosition();
            }
        }

        private void OnListenerExited(IndoorRoomComponent room, RoomState state)
        {
            state.ListenerInside = false;
            var portal = GetPortalForRoom(room);
            foreach (var emitter in state.Emitters)
            {
                if (portal != null && portal.IsOpen && emitter is IOcclusionEmitter pos)
                    pos.OverridePosition(portal.Position);
                else if (portal == null || !portal.IsOpen)
                {
                    if (emitter is BaseAudioEventComponent baseComp) baseComp.Stop();
                }
            }
        }

        private void UnregisterRoom(IndoorRoomComponent room)
        {
            if (_roomStates.TryGetValue(room, out var state))
            {
                foreach (var emitter in state.Emitters)
                {
                    if (emitter is IOcclusionEmitter pos)
                        pos.RestoreOriginalPosition();
                }
                _roomStates.Remove(room);
            }
        }

        private void UpdateRooms(long _)
        {
            if (_listener == null)
                return;

            Vector3 listenerPos = _listener.position;

            foreach (var room in _zoneRegistry.IndoorRooms)
            {
                float distance = Vector3.Distance(listenerPos, room.RoomCollider.bounds.center);

                if (distance > _maxDistance)
                    continue;

                bool listenerInside = room.RoomCollider.bounds.Contains(listenerPos);
                IPortalOpening portal = GetPortalForRoom(room);

                if (!_roomStates.TryGetValue(room, out var state))
                    continue;

                if (listenerInside)
                {
                    // Слушатель внутри: восстанавливаем исходные позиции
                    foreach (var emitter in state.Emitters)
                    {
                        emitter.RestoreOriginalPosition();
                    }
                }
                else
                {
                    // Слушатель снаружи: если портал открыт, перемещаем на него
                    if (portal != null && portal.IsOpen)
                    {
                        foreach (var emitter in state.Emitters)
                        {
                           emitter.OverridePosition(portal.Position);
                           // Применяем RTPC расстояния до портала
                           if (state.OriginalPositions.TryGetValue(emitter, out var origPos))
                           {
                                float dist = Vector3.Distance(origPos, portal.Position);
                                //_audioFacade.SetParameter(AudioParameterAsset.WwiseParameter, dist, emitter.EmitterTransform.gameObject);
                           }
                        }
                    }
                    else
                    {
                        // Нет портала или закрыт — останавливаем звуки
                        foreach (var emitter in state.Emitters)
                        {
                            if (emitter is BaseAudioEventComponent baseComp)
                                baseComp.Stop();

                            else if (emitter is AudioEntity entity)
                                entity.OnDespawned();
                        }
                    }
                }
            }
        }

        private IPortalOpening GetPortalForRoom(IndoorRoomComponent room)
        {
            return _portals.FirstOrDefault(p => p.Room == room);
        }

        public void RegisterPortal(PortalOpeningComponent portal)
        {
            if (!_portals.Contains(portal))
                _portals.Add(portal);
        }

        public void UnregisterPortal(PortalOpeningComponent portal)
        {
            _portals.Remove(portal);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _roomStates.Clear();
            _portals.Clear();
        }
    }
}
