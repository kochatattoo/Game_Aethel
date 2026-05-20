using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    /// <summary>
    /// Компонент тригер, который активирует звуковой ивент (используем на колайдерах, что бы пререключить звук) 
    /// </summary>
    public class TagTriggerAudioComponent : BaseAudioEventComponent
    {
        [Header("Trigger Options")]
        [SerializeField]
        protected bool _playOnEnter = true;
        [SerializeField] 
        protected bool _stopOnExit = false;

        [Tooltip("Тег объекта, который должен активировать триггер")]
        [SerializeField]
        protected string _activationTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(_activationTag) || other.CompareTag(_activationTag))
            {
                if (_playOnEnter)
                {
                    Play();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrEmpty(_activationTag) || other.CompareTag(_activationTag))
            {
                if (_stopOnExit)
                {
                    Stop();
                }
            }
        }
    }
}
