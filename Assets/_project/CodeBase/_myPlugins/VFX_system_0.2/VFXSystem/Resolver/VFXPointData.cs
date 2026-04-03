using UnityEngine;
using VFXSystem.BaseTypes;

namespace VFXSystem.Resolver
{
    public struct VFXPointData
    {
        public MaterialType MaterialType;
        public GameObject GameObject;
        public Vector3 Position;
        public Vector3 Normal;
        public float Interact;

        public VFXPointData(GameObject gameObject, Vector3 position, Vector3 normal, float interact = 1, MaterialType materialType = MaterialType.NoneDetected)
        {
            GameObject = gameObject;
            Position = position;
            Normal = normal;
            Interact = interact;
            MaterialType = materialType; 
        }

        public VFXPointData(GameObject gameObject, Vector3 position, Vector3 normal, MaterialType materialType, float interact = 1)
        {
            GameObject = gameObject;
            Position = position;
            Normal = normal;
            Interact = interact;
            MaterialType = materialType;
        }
    }
}
