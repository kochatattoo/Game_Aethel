using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "StaticData/Hero")]
    public class HeroStaticData : ScriptableObject
    {
        public HeroTypeID HeroTypeId;

        [Range(0f, 10f)]
        public float MoveSpeed;

        [Range(1, 100)]
        public int Hp;

        [Range(1f, 30)]
        public float Damage = 5f;

        [Range(0.5f, 2)]
        public float DamageRadius = 1f;

        [Range(0.5f, 1)]
        public float Cleavage;

        public AssetReferenceGameObject PrefabReference;
    }
}
