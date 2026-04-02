using Infrastructure.AudioSystem.Events;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    public class BushAudioComponent : TriggerAudioComponent
    {
        [Header("Bush Settings")]
        [SerializeField]
        private AudioParameterAsset _speedRtpc;

        [SerializeField]
        private float _minSpeedToSound = 0.1f;
        [SerializeField]
        private float _smoothSpeedTime = 0.2f;

        private bool _isPlayerInside;
        private float _currentVelocity;

        public override void Play()
        {
            base.Play();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (_audioMaker != null)
            {
                _isPlayerInside = true;
            }

        }

        private void Update()
        {
            if (!_isPlayerInside)
                return;

            float speed = _audioMaker.CurrentSpeed;

            float displaySpeed = Mathf.SmoothDamp(_currentVelocity, speed, ref _currentVelocity, _smoothSpeedTime);

            float finalSpeed = displaySpeed > _minSpeedToSound ? displaySpeed : 0;

            _audioFacade.SetParameter(_speedRtpc.WwiseParameter, finalSpeed, gameObject);
        }

        protected override void OnTriggerExit(Collider other)
        {
            _isPlayerInside = false;

            _audioFacade.SetParameter(_speedRtpc.WwiseParameter, 0, gameObject);

            base.OnTriggerExit(other);
        }
    }
}
