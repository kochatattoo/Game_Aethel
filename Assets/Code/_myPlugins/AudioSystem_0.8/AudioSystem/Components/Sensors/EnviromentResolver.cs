using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Parameters.DTO;
using Infrastructure.AudioSystem.Zones;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Sensors
{
    public class EnviromentResolver
    {
        private readonly GameObject _audioMakerObject;
        private readonly IAudioFacade _audioFacade;
        private readonly List<IEnvironmentComponent> _activeEnvironments = new();
        private readonly List<IEnvironmentComponent> _activeRooms = new();
        private readonly IAudioZoneRegistry _zoneRegistry;

        public bool IsInPortal { get ; set; }

        public EnviromentResolver(GameObject audioMakerObject, IAudioFacade audioFacade, IAudioZoneRegistry zoneRegistry)
        {
            _audioMakerObject = audioMakerObject;
            _audioFacade = audioFacade;
            _zoneRegistry = zoneRegistry;
        }

        public void InitializeZones()
        {
            if (_zoneRegistry == null || _audioMakerObject == null) 
                return;

            Collider myCollider = _audioMakerObject.GetComponent<Collider>();
            if (myCollider == null)
            {
                Vector3 pos = _audioMakerObject.transform.position;

                foreach (var env in _zoneRegistry.Environments)
                {
                    if (env != null && env.GetComponent<Collider>()?.bounds.Contains(pos) == true)
                        EnterEnvironment(env);
                }

                foreach (var room in _zoneRegistry.Rooms)
                {
                    if (room != null && room.GetComponent<Collider>()?.bounds.Contains(pos) == true)
                        EnterRoom(room);
                }
            }
            else
            {
                Bounds myBounds = myCollider.bounds;
                foreach (var env in _zoneRegistry.Environments)
                {
                    if (env != null && env.GetComponent<Collider>()?.bounds.Intersects(myBounds) == true)
                        EnterEnvironment(env);
                }
                foreach (var room in _zoneRegistry.Rooms)
                {
                    if (room != null && room.GetComponent<Collider>()?.bounds.Intersects(myBounds) == true)
                        EnterRoom(room);
                }
            }
            UpdateAuxSend();
        }

        public void EnterEnvironment(EnvironmentAudioComponent env)
        {
            if (env == null)
                return;
            _activeEnvironments.Add(env);

            if (!IsInPortal)
                UpdateAuxSend();
        }

        public void ExitEnvironment(EnvironmentAudioComponent env)
        {
            if (env == null)
                return;

            _activeEnvironments.Remove(env);

            if (!IsInPortal)
                UpdateAuxSend();
        }

        public void EnterRoom(IRoomComponent room)
        {
            if (room is IEnvironmentComponent envRoom)
            {
                _activeRooms.Add(envRoom);
                UpdateAuxSend();
            }
        }

        public void ExitRoom(IRoomComponent room)
        {
            if (room is IEnvironmentComponent envRoom)
            {
                _activeRooms.Remove(envRoom);
                UpdateAuxSend();
            }
        }

        [CanBeNull]
        public AK.Wwise.AuxBus GetCurrentAuxBus()
        {
            if (IsInPortal)
                return null;

            var highestPriorityEnv = _activeEnvironments
                .Cast<IEnvironmentComponent>()
                .Concat(_activeRooms)
                .Where(e => e?.AuxBus != null)
                .OrderByDescending(e => e.Priority)
                .FirstOrDefault();

            return highestPriorityEnv?.AuxBus;
        }

        public AuxSendData GetCurrentAuxSendData()
        {
            var bus = GetCurrentAuxBus();
            if (bus != null)
            {
                return new AuxSendData
                {
                    AuxBusA = bus,
                    VolumeA = 1f,
                    IsBlended = false
                };
            }
            return default;
        }

        public void UpdateAuxSend()
        {
            if (_audioMakerObject == null || !_audioMakerObject) 
                return;

            if (IsInPortal)
                return;

            // Удаляем мёртвые ссылки
            _activeEnvironments.RemoveAll(e => e == null);
            _activeRooms.RemoveAll(r => r == null);

            // Объединяем все активные зоны и комнаты и находим наивысший приоритет
            IEnvironmentComponent best = _activeEnvironments
                .Cast<IEnvironmentComponent>()
                .Concat(_activeRooms)
                .Where(e => e?.AuxBus != null)
                .OrderByDescending(e => e.Priority)
                .FirstOrDefault();

            if (best != null)
                _audioFacade?.SetGameObjectAuxSend(_audioMakerObject, best.AuxBus);
            else
                _audioFacade?.ResetGameObjectAuxSend(_audioMakerObject);
        }
    }
}
