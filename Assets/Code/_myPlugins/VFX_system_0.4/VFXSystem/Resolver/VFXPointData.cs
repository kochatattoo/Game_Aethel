using UnityEngine;
using VFXSystem.BaseTypes;

namespace VFXSystem.Resolver
{
    public readonly struct VFXPointData
    {
        public readonly MaterialType MaterialType;
        public readonly GameObject GameObject;
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Quaternion HitRotation;
        public readonly Transform Parent;
        public readonly float Interact;

        public VFXPointData(GameObject gameObject, 
            Vector3 position, 
            Vector3 normal,
            Quaternion hitRotation,
            float interact = 1, 
            MaterialType materialType = MaterialType.None)
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
