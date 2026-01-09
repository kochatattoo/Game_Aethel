using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "StaticData/Monster")]
    public class MonsterStaticData : ScriptableObject
    {
        public MonsterTypeID MonsterTypeId;

        [Range(1, 100)]
        public int Hp;

        [Range(1f, 30)]
        public float Damage = 5f;

        public int MaxLoot;
        public int MinLoot;

        [Range(0f, 10f)]
        public float MoveSpeed;

        [Range(0.5f, 5)]
        public float EffectiveDistance = 0.5f;

        [Range(0.5f, 5)]
        public float Radius;

        public AssetReferenceGameObject PrefabReference;
    }
}
