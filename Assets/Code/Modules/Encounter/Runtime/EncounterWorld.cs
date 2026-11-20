using PetProject.Navigation;
using Unity.AI.Navigation;
using UnityEngine;

namespace PetProject.Encounter
{
    public sealed class EncounterWorld : MonoBehaviour
    {
        public NavigationConfig Config;
        public Transform GeneratedRoot;
        public NavMeshSurface Surface;
        [HideInInspector] public string BakedSignature;

        public string Signature
        {
            get
            {
                var geometry = 17;
                if (GeneratedRoot != null)
                    foreach (var collider in GeneratedRoot.GetComponentsInChildren<Collider>())
                    {
                        unchecked
                        {
                            geometry = geometry * 31 + collider.bounds.center.GetHashCode();
                            geometry = geometry * 31 + collider.bounds.size.GetHashCode();
                            var marker = collider.GetComponent<NavigationSurface>();
                            geometry = geometry * 31 + (marker != null ? marker.Cost : 0);
                        }
                    }
                var settings = UnityEngine.AI.NavMesh.GetSettingsByID(Config.AgentTypeId);
                return $"{Config.Seed}:{Config.WorldSize:R}:{Config.CellSize:R}:{Config.OriginX:R}:{Config.OriginZ:R}:" +
                    $"{Config.AgentTypeId}:{Config.AgentRadius:R}:{Config.AgentHeight:R}:{settings.agentSlope:R}:{settings.agentClimb:R}:{geometry}";
            }
        }
    }
}
