using Unity.AI.Navigation;
using UnityEngine;

namespace CodeBase.MotionTest
{
    public sealed class MotionTestWorld : MonoBehaviour
    {
        public MotionTestConfig Config;
        public Transform GeneratedRoot;
        public NavMeshSurface Surface;
        [HideInInspector] public string BakedSignature;

        public string Signature
        {
            get
            {
                // Bounds are value data, unlike instance IDs they survive scene/domain reloads.
                var geometry = 17;
                if (GeneratedRoot != null)
                    foreach (var collider in GeneratedRoot.GetComponentsInChildren<Collider>())
                    {
                        unchecked
                        {
                            geometry = geometry * 31 + collider.bounds.center.GetHashCode();
                            geometry = geometry * 31 + collider.bounds.size.GetHashCode();
                            var marker = collider.GetComponent<MotionSurface>();
                            geometry = geometry * 31 + (marker != null ? marker.Cost : 0);
                        }
                    }
                var settings = UnityEngine.AI.NavMesh.GetSettingsByID(Config.AgentTypeId);
                return $"{Config.Seed}:{Config.WorldSize:R}:{Config.CellSize:R}:{Config.AgentTypeId}:{Config.AgentRadius:R}:{Config.AgentHeight:R}:{settings.agentSlope:R}:{settings.agentClimb:R}:{geometry}";
            }
        }

        // The editor owns replacement/Undo. This method only creates a fresh, explicitly owned root.
        public void Generate()
        {
            if (GeneratedRoot != null) throw new System.InvalidOperationException("Remove the previous generated root with Undo before generating");
            GeneratedRoot = new GameObject("GeneratedRoot").transform;
            GeneratedRoot.SetParent(transform, false);
            Box("Ground 100 x 100", new Vector3(0, -.5f, 0), new Vector3(Config.WorldSize, 1, Config.WorldSize), Config.Floor);
            Block("Cube obstacle", new Vector3(-5, 1.5f, -12), new Vector3(4, 3, 4));
            Block("Long wall", new Vector3(-1, 2, -5), new Vector3(2, 4, 12));
            Block("Thin wall between cells", new Vector3(-12, 1.5f, -8), new Vector3(.2f, 3, 5));
            Sphere("Sphere obstacle", new Vector3(-8, 2, -2), 4);
            Sphere("Small sphere obstacle", new Vector3(0, 1, -15), 2);
            // Wide corridor, plus a separate corridor narrower than the actor diameter.
            Block("Wide passage left", new Vector3(-20, 2, 5), new Vector3(1, 4, 12));
            Block("Wide passage right", new Vector3(-16, 2, 5), new Vector3(1, 4, 12));
            Block("Narrow passage left", new Vector3(-10, 2, 8), new Vector3(1, 4, 10));
            Block("Narrow passage right", new Vector3(-8.3f, 2, 8), new Vector3(1, 4, 10));
            Ramp("Gentle ramp 14 degrees", 8, -2, 8, 2, 6);
            Box("Gentle upper platform", new Vector3(8, 1, 9), new Vector3(6, 2, 6), Config.Platform);
            Ramp("Medium ramp 27 degrees", 20, -2, 8, 4, 6);
            Box("Medium upper platform", new Vector3(20, 2, 9), new Vector3(6, 4, 6), Config.Platform);
            Ramp("Too steep ramp 63 degrees", 32, 0, 4, 8, 6);
            Box("High isolated platform", new Vector3(32, 4, 7), new Vector3(6, 8, 6), Config.Platform);
            Block("Unreachable north wall", new Vector3(-30, 2, 26), new Vector3(12, 4, 1));
            Block("Unreachable south wall", new Vector3(-30, 2, 14), new Vector3(12, 4, 1));
            Block("Unreachable west wall", new Vector3(-36, 2, 20), new Vector3(1, 4, 13));
            Block("Unreachable east wall", new Vector3(-24, 2, 20), new Vector3(1, 4, 13));
            // A thin, explicitly walkable patch carries movement cost in the Unity-to-grid adapter.
            var costly = Box("Cost 2 terrain", new Vector3(-12, .025f, -20), new Vector3(7, .05f, 5), Config.Expensive);
            costly.GetComponent<MotionSurface>().Cost = 2;
            var random = new System.Random(Config.Seed);
            for (var i = 0; i < 24; ++i)
            {
                var p = new Vector3((float)random.NextDouble() * 80 - 40, 1, 33 + (float)random.NextDouble() * 12);
                Block("Seeded block " + i, p, new Vector3(1.5f, 2, 1.5f));
            }
            Surface = GeneratedRoot.gameObject.AddComponent<NavMeshSurface>();
            Surface.agentTypeID = Config.AgentTypeId;
            Surface.collectObjects = CollectObjects.Children;
            Surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            Surface.overrideVoxelSize = true;
            Surface.voxelSize = Config.AgentRadius / 4;
            BakedSignature = "";
        }

        private GameObject Box(string name, Vector3 position, Vector3 size, Material material) =>
            Primitive(PrimitiveType.Cube, name, position, size, material, true);
        private void Block(string name, Vector3 position, Vector3 size) =>
            Primitive(PrimitiveType.Cube, name, position, size, Config.Obstacle, false);
        private void Sphere(string name, Vector3 position, float diameter) =>
            Primitive(PrimitiveType.Sphere, name, position, Vector3.one * diameter, Config.Decoration, false);
        private GameObject Primitive(PrimitiveType type, string name, Vector3 p, Vector3 size, Material material, bool walkable)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = name; item.transform.SetParent(GeneratedRoot, false);
            item.transform.position = p; item.transform.localScale = size;
            item.GetComponent<Renderer>().sharedMaterial = material;
            item.layer = walkable ? LayerMask.NameToLayer("Ground") : 0;
            var marker = item.AddComponent<MotionSurface>(); marker.Walkable = walkable;
            if (!walkable)
            {
                var modifier = item.AddComponent<NavMeshModifier>();
                modifier.overrideArea = true; modifier.area = 1;
            }
            return item;
        }
        private void Ramp(string name, float x, float startZ, float run, float rise, float width)
        {
            var rotation = Quaternion.Euler(-Mathf.Atan2(rise, run) * Mathf.Rad2Deg, 0, 0);
            var center = new Vector3(x, rise * .5f, startZ + run * .5f) - rotation * Vector3.up * .15f;
            var item = Box(name, center, new Vector3(width, .3f, Mathf.Sqrt(run * run + rise * rise)), Config.Ramp);
            item.transform.rotation = rotation;
        }
    }
}
