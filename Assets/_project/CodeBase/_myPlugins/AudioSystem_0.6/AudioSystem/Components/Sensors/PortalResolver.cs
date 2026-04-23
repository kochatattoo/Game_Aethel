using Infrastructure.AudioSystem.Parameters.DTO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Sensors
{
    public class PortalResolver
    {
        private readonly Transform _trackedTransform;
        private readonly List<PortalEntry> _activePortals = new();

        public bool IsInPortal => _activePortals.Count > 0;

        public PortalResolver(Transform trackedTransform)
        {
            _trackedTransform = trackedTransform;
        }

        public void EnterPortal(EnvironmentPortalComponent portal, EnvironmentAudioComponent envA, EnvironmentAudioComponent envB)
        {
            _activePortals.Add(new PortalEntry(portal, envA, envB));
        }

        public void ExitPortal(EnvironmentPortalComponent portal)
        {
            _activePortals.RemoveAll(p => p.Portal == portal);
        }

        public AuxSendData GetBlendedAuxSendData()
        {
            if (_activePortals.Count == 0)
                return default;

            var nearest = _activePortals
               .Select(p => new { Entry = p, Distance = p.Portal.CalculateDistance(_trackedTransform.position) })
               .OrderBy(x => x.Distance)
               .First().Entry;

            return nearest.GetBlendedAuxSendData(_trackedTransform.position);
        }

        private class PortalEntry
        {
            public EnvironmentPortalComponent Portal;
            public EnvironmentAudioComponent EnvA;
            public EnvironmentAudioComponent EnvB;

            public PortalEntry(EnvironmentPortalComponent portal, EnvironmentAudioComponent envA, EnvironmentAudioComponent envB)
            {
                Portal = portal;
                EnvA = envA;
                EnvB = envB;
            }

            public AuxSendData GetBlendedAuxSendData(Vector3 position)
            {
                float weight = Portal.CalculateWeight(position);
                return new AuxSendData
                {
                    AuxBusA = EnvA.AuxBus,
                    VolumeA = 1f - weight,
                    AuxBusB = EnvB.AuxBus,
                    VolumeB = weight,
                    IsBlended = true
                };
            }
        }
    }
}
