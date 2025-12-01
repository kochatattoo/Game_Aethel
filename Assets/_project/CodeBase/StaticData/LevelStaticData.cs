using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "StaticData/level")]
    public class LevelStaticData : ScriptableObject
    {
        public string LevelKey;
        public Vector3 InitialHeroPosition;

    }
}
