using UnityEngine;

namespace CodeBase.MotionTest
{
    internal static class GridPathMovement
    {
        public static bool MoveStep(Transform actor, SurfaceGridMap map, Vector3 destination, float speed)
        {
            var current = actor.position;
            var step = Vector3.MoveTowards(new Vector3(current.x, 0, current.z),
                new Vector3(destination.x, 0, destination.z), speed * Time.deltaTime);
            if (!map.Support(step, out var supported, out _)) return false;

            actor.position = supported;
            var forward = new Vector3(destination.x - current.x, 0, destination.z - current.z);
            if (forward.sqrMagnitude > .0001f)
                actor.rotation = Quaternion.RotateTowards(actor.rotation, Quaternion.LookRotation(forward),
                    540 * Time.deltaTime);
            if (!HasArrived(actor, destination)) return true;

            actor.position = destination;
            return true;
        }

        public static bool HasArrived(Transform actor, Vector3 destination) =>
            Vector2.Distance(new Vector2(actor.position.x, actor.position.z),
                new Vector2(destination.x, destination.z)) <= .005f;
    }
}
