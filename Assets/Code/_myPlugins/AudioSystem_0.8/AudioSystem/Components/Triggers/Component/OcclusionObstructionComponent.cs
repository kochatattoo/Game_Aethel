using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    /// <summary>
    /// Помечает объект как препятствие для звука с заданным уровнем obstruction (0..1).
    /// </summary>
    public class OcclusionObstructionComponent : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _baseObstructionValue = 1f;

        [Tooltip("Кривая зависимости obstruction от расстояния (ось X = расстояние до препятствия, Y = obstruction)")]
        [SerializeField] private AnimationCurve _obstructionCurve;

        public float GetObstructionValue(float distanceToObstacle)
        {
            if (_obstructionCurve != null && _obstructionCurve.keys.Length > 0)
                return Mathf.Clamp01(_obstructionCurve.Evaluate(distanceToObstacle));

            return _baseObstructionValue;
        }
    }
}
