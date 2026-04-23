using System;
using System.Collections.Generic;
using UnityEngine;
using VFXSystem.BaseTypes;

namespace VFXSystem.Parameters.MaterialMap
{
    [Serializable]
    public class VFXToMaterial
    {
        [SerializeField]
        private MaterialType _material;

        [SerializeField]
        private List<Material> _materials = new();

        public MaterialType Material => _material;

        public IReadOnlyList<Material> Materials => _materials;
    }
}
