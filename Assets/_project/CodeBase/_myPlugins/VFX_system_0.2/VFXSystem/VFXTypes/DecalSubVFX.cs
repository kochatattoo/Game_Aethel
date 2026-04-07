using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VFXSystem.VFXTypes
{
    /// <summary>
    /// Управляет визуальным отображением декалей (отпечатков) на поверхностях.
    /// Отвечает за логику появления, масштабирования от силы удара и последующего скрытия.
    /// </summary>
    public class DecalSubVFX : MonoBehaviour, ISubVFX
    {
        [SerializeField]
        private float _duration = 5f;
        [SerializeField]
        private float _fadeOutTime = 1.5f; // Время плавного исчезновения в конце
        [SerializeField]
        private DecalProjector _projector;
        private Coroutine _fadeCoroutine;

        public GameObject GameObject => gameObject;

        public float Duration => _duration; // Время до исчезновения

        public void Play(float impactScale)
        {
            Stop();

            // Декаль становится больше от сильного удара
            transform.localScale = Vector3.one * impactScale;
            gameObject.SetActive(true);

            // Сбрасываем прозрачность в 1
            if (_projector != null)
            {
                _projector.fadeFactor = 1f;
                // Устанавливаем размер проектора на основе силы удара
                _projector.size = new Vector3(impactScale, impactScale, _projector.size.z);
            }

            // Запускаем цикл жизни
            _fadeCoroutine = StartCoroutine(DecalLifeCycle());
        }

        public void Stop()
        {
            if (_fadeCoroutine != null) 
                StopCoroutine(_fadeCoroutine);
            gameObject.SetActive(false);
        }

        private IEnumerator DecalLifeCycle()
        {
            // Ждем основное время жизни минус время затухания
            float waitTime = Mathf.Max(0, _duration - _fadeOutTime);
            yield return new WaitForSeconds(waitTime);

            // Плавное затухание
            if (_projector != null)
            {
                float elapsed = 0;
                while (elapsed < _fadeOutTime)
                {
                    elapsed += Time.deltaTime;
                    _projector.fadeFactor = Mathf.Lerp(1f, 0f, elapsed / _fadeOutTime);
                    yield return null;
                }
            }
        }
    }
}
