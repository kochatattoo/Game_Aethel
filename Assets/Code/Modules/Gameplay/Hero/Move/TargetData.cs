using UnityEngine;

namespace CodeBase.Hero
{
    public enum TargetType
    {
        None,
        Move,
        Attack,
        Interact
    }

    public class TargetData
    {
        public TargetType Type { get; }
        public Vector3 Position { get; }
        public GameObject HitObject { get; }

        public TargetData(TargetType type, Vector3 position, GameObject hitObject = null)
        {
            Type = type;
            Position = position;
            HitObject = hitObject;
        }
    }
}
