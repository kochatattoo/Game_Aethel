using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GoogleSheetConfig
{
    [Serializable]
    public class ParserEntry
    {
        [HideInInspector]
        public ISettingsParser Parser;

        [ReadOnly]
        public string Sheet;
            
        [LabelWidth(120)]
        public bool NeedLoaded = true;
    }
}