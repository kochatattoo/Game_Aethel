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
        private readonly List<EnvironmentAudioComponent> _activeEnvironments = new();

        public bool IsInPortal { get ; set; }

        public EnviromentResolver(GameObject audioMakerObject, IAudioFacade audioFacade)
        {
            _audioMakerObject = audioMakerObject;
            _audioFacade = audioFacade;
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

        [CanBeNull]
        public AK.Wwise.AuxBus GetCurrentAuxBus()
        {
            if (IsInPortal) 
                return null;

            var highestPriorityEnv = _activeEnvironments
                .OrderByDescending(e => e.Priority)
                .FirstOrDefault();
            return highestPriorityEnv?.AuxBus;
        }

        public void UpdateAuxSend()
        {
            if (_audioMakerObject == null || !_audioMakerObject)
                return;

            if (IsInPortal) 
                return;

            _activeEnvironments.RemoveAll(e => e == null || !e);

            var highestPriorityEnv = _activeEnvironments
                .OrderByDescending(e => e.Priority)
                .FirstOrDefault();

            if (highestPriorityEnv != null)
                _audioFacade?.SetGameObjectAuxSend(_audioMakerObject, highestPriorityEnv.AuxBus);
            else
                _audioFacade?.ResetGameObjectAuxSend(_audioMakerObject);
        }
    }
}
