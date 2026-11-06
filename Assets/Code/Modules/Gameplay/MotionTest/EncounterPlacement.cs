using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.MotionTest
{
    internal static class EncounterPlacement
    {
        public static bool FindCell(SurfaceGridMap map, MotionTestConfig config, Vector3 from,
            GridPosition? excluded, out GridPosition destination)
        {
            map.Cell(from, out var origin);
            var best = float.MaxValue;
            destination = default;
            var found = false;
            var filter = new NavMeshQueryFilter { agentTypeID = config.AgentTypeId, areaMask = NavMesh.AllAreas };
            var path = new NavMeshPath();
            for (var z = origin.Y - 3; z <= origin.Y + 3; ++z)
            for (var x = origin.X - 3; x <= origin.X + 3; ++x)
            {
                var cell = new GridPosition(x, z);
                if (!map.Board.Contains(cell) || !map.Board.GetCell(cell).Walkable || cell == excluded) continue;
                var point = map.World(cell);
                var distance = (from - point).sqrMagnitude;
                if (distance > 16 || distance >= best) continue;
                if (!NavMesh.CalculatePath(from, point, filter, path) ||
                    path.status != NavMeshPathStatus.PathComplete) continue;
                destination = cell;
                best = distance;
                found = true;
            }
            return found;
        }

        public static bool HasArrived(NavMeshAgent agent, Vector3 destination) =>
            agent.enabled && agent.isOnNavMesh && !agent.pathPending &&
            agent.pathStatus == NavMeshPathStatus.PathComplete &&
            (agent.remainingDistance <= .08f || Vector3.Distance(agent.transform.position, destination) < .08f);
    }
}
