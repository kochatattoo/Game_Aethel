using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{
    [Serializable]
    public class WwiseToMaterial<T>
    {
        [SerializeField]
        private T _switchValue;

        [SerializeField]
        private List<Material> _materials = new();

        public T SwitchValue => _switchValue;

        public IReadOnlyList<Material> Materials => _materials;
    }
}
