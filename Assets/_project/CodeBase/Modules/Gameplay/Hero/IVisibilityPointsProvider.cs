using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Hero
{
    public interface IVisibilityPointsProvider
    {
        public Transform OriginOfRays { get; }
        IReadOnlyList<Transform> Points { get; }
    }
}

