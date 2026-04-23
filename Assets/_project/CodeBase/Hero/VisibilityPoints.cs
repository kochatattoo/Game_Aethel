using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Hero
{
    public class VisibilityPoints : MonoBehaviour, IVisibilityPointsProvider
    {
        [SerializeField, Tooltip("Точки определения видимости юнита. " +
                                 "По большей части это нужно для NPC, помощь в определении видимости игрока. " +
                                 "Рекомендую использовать 4 точки: голова, тело, руки")]
        private List<Transform> _points = new();
        [field: SerializeField, Tooltip("Transform откуда мы отправляем лучи для нахождения точек")]
        public Transform OriginOfRays { get; private set; }

        public IReadOnlyList<Transform> Points => _points;
    }
}

