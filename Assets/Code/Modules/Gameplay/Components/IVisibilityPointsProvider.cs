using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Components
{
    public interface IVisibilityPointsProvider
    {
        public Transform OriginOfRays { get; }
        IReadOnlyList<Transform> Points { get; }
    }
}

