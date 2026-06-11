using Shared.Utils.Constants;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem.Banks
{
    [CreateAssetMenu(fileName = nameof(BankSceneMapping), menuName = ScriptableObjectNames.AudioMaping + nameof(BankSceneMapping))]
    public class BankSceneMapping : ScriptableObject
    {
        [SerializeField]
        private List<BankSceneMappingData> _banks = new();

        public IReadOnlyList<BankSceneMappingData> Banks => _banks;

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var data in _banks)
                data.OnValidate();
        }
#endif
    }
}
