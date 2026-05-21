using DialogueSystemExtensions.Components.Makers;
using UnityEngine;

namespace DialogueSystemExtensions.Components.Sensors
{
    [RequireComponent(typeof(Collider))]
    public abstract class TriggerSensor: MonoBehaviour
    {
        [SerializeField]
        private Collider _collider;
        protected IDialogueMaker _dialogueMaker;

        private void Awake()
        {
            if( _collider == null )
                _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IDialogueMaker>(out IDialogueMaker maker))
            {
                _dialogueMaker = maker;
                OnEnter(maker);
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IDialogueMaker>(out IDialogueMaker maker))
                OnExit(maker);

            _dialogueMaker = null;
        }

        protected abstract void OnEnter(IDialogueMaker maker);
        protected abstract void OnExit(IDialogueMaker maker);

    }
}
