using UnityEngine;

namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Компонент для второстепенных эффектов, таких как вспышки света (Light).
    /// Гарантирует корректное отключение источников света при возврате объекта в пул.
    /// </summary>
    public class AdditionalSubVFX : MonoBehaviour, ISubVFX
    {
        [Header("Components")]
        [SerializeField] 
        private Light _impactLight;

        [Header("Settings")]
        [SerializeField] 
        private float _duration = 0.2f;

        public float Duration => _duration;

        public GameObject GameObject => gameObject;

        public void Play(float impactScale)
        {
            gameObject.SetActive(true);

            if (_impactLight != null)
            {
                _impactLight.enabled = true;

                Invoke(nameof(DisableLight), _duration);
            }
        }

        public void Stop()
        {
            CancelInvoke();

            if (_impactLight != null) 
                _impactLight.enabled = false;

            gameObject.SetActive(false);
        }

        private void DisableLight()
        {
            if (_impactLight != null) 
                _impactLight.enabled = false;
        }
    }
}