using UnityEngine;

namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Управляет визуальным отображением декалей (отпечатков) на поверхностях.
    /// Отвечает за логику появления, масштабирования от силы удара и последующего скрытия.
    /// </summary>
    public class DecalSubVFX : MonoBehaviour, ISubVFX
    {
        [SerializeField]
        private float _duration;

        public GameObject GameObject => gameObject;

        public float Duration => _duration; // Время до исчезновения

        public void Play(float impactScale)
        {
            // Декаль становится больше от сильного удара
            transform.localScale = Vector3.one * impactScale;
            gameObject.SetActive(true);
        }

        public void Stop() => gameObject.SetActive(false);

    }
}
