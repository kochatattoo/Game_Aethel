using Infrastructure.AudioSystem.Events;
using System;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    [Obsolete("Tag logic doens't implement, use Component")]
    public class TagBushAudioComponent : TagTriggerAudioComponent
    {
        [Header("Bush Settings")]
        [SerializeField]
        private AudioParameterAsset _speedRtpc;

        [SerializeField]
        private float _minSpeedToSound = 0.1f;
        [SerializeField]
        private float _smoothSpeedTime = 0.2f;

        private Rigidbody _playerRigidbody;
        private bool _isPlayerInside;
        private float _currentVelocity;

        public override void Play()
        {
            // Для кустов лучше использовать зацикленный звук (Loop), 
            // который мы просто "открываем" по громкости через RTPC
            base.Play();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_activationTag))
            {
                _playerRigidbody = other.GetComponent<Rigidbody>();
                _isPlayerInside = true;
                Play();
            }
        }

        private void Update()
        {
            if (!_isPlayerInside || _playerRigidbody == null) 
                return;

            // Получаем текущую скорость игрока
            float speed = _playerRigidbody.linearVelocity.magnitude;

            // Сглаживаем значение, чтобы звук не "прыгал"
            float displaySpeed = Mathf.SmoothDamp(_currentVelocity, speed, ref _currentVelocity, _smoothSpeedTime);

            // Если игрок замер — звук затихает (RTPC в 0), если бежит — шуршит громко
            float finalSpeed = displaySpeed > _minSpeedToSound ? displaySpeed : 0;

            _audioFacade.SetParameter(_speedRtpc.WwiseParameter, finalSpeed, gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_activationTag))
            {
                _isPlayerInside = false;
                _playerRigidbody = null;

                // Плавно зануляем параметр перед остановкой, чтобы не было щелчка
                _audioFacade.SetParameter(_speedRtpc.WwiseParameter, 0, gameObject);
                Stop();
            }
        }
    }
}
