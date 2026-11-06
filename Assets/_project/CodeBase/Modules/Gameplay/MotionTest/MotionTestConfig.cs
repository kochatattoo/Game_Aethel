using UnityEngine;

namespace CodeBase.MotionTest
{
    [CreateAssetMenu(menuName = "PetProject/Tests/Motion config")]
    public sealed class MotionTestConfig : ScriptableObject
    {
        [Min(10)] public float WorldSize = 100;
        [Min(.5f)] public float CellSize = 1;
        public int Seed = 718;
        [Min(1)] public int ActionBudget = 6;
        [Min(1)] public int VisibleRadius = 12;
        public int AgentTypeId = 1479372276;
        public float AgentRadius = .5f;
        public float AgentHeight = 3.56f;
        public float Speed = 5;
        public float HeightUnit = .25f;
        public int MaxHeightStep = 4;
        public Vector3 Spawn = new Vector3(-16.5f, 0, -15.5f);
        public Material Floor, Obstacle, Ramp, Platform, Expensive, Decoration, GridMaterial;
        public Color GridColor = new Color(.3f, .8f, .95f, .28f);
        public Color ExpensiveCellColor = new Color(1, .65f, .2f, .4f);
        public Color CurrentCellColor = new Color(.2f, 1, .7f, .9f);
        public Color PathAvailableColor = new Color(.25f, 1, .65f, 1);
        public Color PathUnavailableColor = new Color(1, .25f, .25f, 1);
        public Color PathExecutingColor = new Color(.3f, .85f, 1, 1);
        public Color HoverColor = Color.yellow;
        public Color TargetHighlightColor = new Color(1, .85f, .1f, 1);
        public int Dimension => Mathf.RoundToInt(WorldSize / CellSize);
        public float Origin => -WorldSize * .5f;
        private void OnValidate()
        {
            CellSize = Mathf.Max(.5f, CellSize); WorldSize = Mathf.Clamp(WorldSize, 10, 200);
            WorldSize = Mathf.Round(WorldSize / CellSize) * CellSize;
            HeightUnit = Mathf.Max(.05f, HeightUnit);
            AgentRadius = Mathf.Max(.1f, AgentRadius);
            AgentHeight = Mathf.Max(AgentRadius * 2, AgentHeight);
            Speed = Mathf.Max(.1f, Speed);
            MaxHeightStep = Mathf.Max(0, MaxHeightStep);
        }
    }
}
