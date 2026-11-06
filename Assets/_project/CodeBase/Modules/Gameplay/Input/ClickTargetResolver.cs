using CodeBase.Hero;
using UnityEngine;

namespace CodeBase.Infrastructure.Services
{
    internal sealed class ClickTargetResolver
    {
        private const float MaxDistance = 200f;
        private static readonly int GroundLayer = 1 << LayerMask.NameToLayer("Ground");
        private static readonly int UnitsLayer = 1 << LayerMask.NameToLayer("Units");
        private static readonly int InteractLayer = 1 << LayerMask.NameToLayer("Interactable");
        private static readonly int DefaultLayer = 1 << LayerMask.NameToLayer("Default");
        private static readonly int TargetMask = GroundLayer | UnitsLayer | InteractLayer | DefaultLayer;

        public TargetData Resolve(Vector3 screenPosition)
        {
            var camera = Camera.main;
            if (camera == null) return new TargetData(TargetType.None, Vector3.zero);
            var ray = camera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, MaxDistance, TargetMask))
                return new TargetData(TargetType.None, Vector3.zero);

            var target = hit.collider.gameObject;
            var layer = 1 << target.layer;
            if ((UnitsLayer & layer) != 0) return new TargetData(TargetType.Attack, hit.point, target);
            if ((InteractLayer & layer) != 0) return new TargetData(TargetType.Interact, hit.point, target);
            if ((GroundLayer & layer) != 0) return new TargetData(TargetType.Move, hit.point, null);
            return null;
        }
    }
}
