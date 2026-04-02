using Infrastructure.AudioSystem.Components.Interfaces;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    public class TriggerAudioComponent : BaseAudioEventComponent
    {
        [Header("Trigger Options")]
        [SerializeField]
        protected bool _playOnEnter = true;
        [SerializeField]
        protected bool _stopOnExit = false;

        protected IAudioMaker _audioMaker;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IAudioMaker>(out IAudioMaker maker))
            {
                _audioMaker = maker;

                if (_playOnEnter)
                {
                    Play();
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IAudioMaker>(out IAudioMaker maker))
            {
                _audioMaker = null;

                if (_stopOnExit)
                {
                    Stop();
                }
            }
        }
    }
}
