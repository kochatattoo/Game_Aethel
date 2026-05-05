using Infrastructure.AudioSystem.Components.Interfaces;
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
        private bool _isUpdating = true;
        [SerializeField]
        private float _minSpeedToSound = 0.1f;

        private bool _isPlayerInside;

        public override void Play()
        {
            base.Play();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IAudioMaker>(out IAudioMaker maker))
            {
                _audioMaker = maker;
                _isPlayerInside = true;
                SetParameter();
            }

            base.OnTriggerEnter(other);
        }

        private void Update()
        {
            if (!_isUpdating || !_isPlayerInside || _audioMaker == null)
                return;

            SetParameter();
        }

        private void SetParameter()
        {
            if (_audioMaker == null || _speedRtpc?.WwiseParameter == null)
                return;

            float speed = _audioMaker.CurrentSpeed;
            float finalSpeed = speed > _minSpeedToSound ? speed : 0;
            Debug.Log($"Final speed{finalSpeed}");

            _audioFacade.SetParameter(_speedRtpc.WwiseParameter, finalSpeed, gameObject);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (_audioMaker != null && _speedRtpc?.WwiseParameter != null)
                _audioFacade.SetParameter(_speedRtpc.WwiseParameter, 0, gameObject);

            _isPlayerInside = false;

            base.OnTriggerExit(other);
        }
    }
}
