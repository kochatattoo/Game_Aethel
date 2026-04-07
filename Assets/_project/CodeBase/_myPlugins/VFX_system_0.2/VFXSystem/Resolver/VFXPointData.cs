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
        public Quaternion HitRotation;
        public Transform Parent;
        public float Interact;

        public VFXPointData(GameObject gameObject, 
            Vector3 position, 
            Vector3 normal,
            Quaternion hitRotation,
            float interact = 1, 
            MaterialType materialType = MaterialType.NoneDetected)
        {
            GameObject = gameObject;
            Position = position;
            Normal = normal;
            HitRotation = hitRotation;
            Interact = interact;
            MaterialType = materialType; 
            Parent = gameObject.transform;
        }

        public VFXPointData(GameObject gameObject, 
            Vector3 position, 
            Vector3 normal, 
            Quaternion hitRotation, 
            MaterialType materialType, 
            float interact = 1)
        {
            GameObject = gameObject;
            Position = position;
            Normal = normal;
            HitRotation = hitRotation;
            Interact = interact;
            MaterialType = materialType;
            Parent = gameObject.transform;
        }
    }
}
