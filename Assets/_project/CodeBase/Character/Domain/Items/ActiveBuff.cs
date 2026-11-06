using Code.Hero.Attributes;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Code.Hero.Items
{
    public class ActiveBuff
    {
        public ConsumableItem SourceItem { get; }
        public List<AttributeModifier> Modifiers { get; }
        public float StartTime { get; }
        public CompositeDisposable Disposables { get; } = new();

        public ActiveBuff(ConsumableItem item, List<AttributeModifier> mods)
        {
            SourceItem = item;
            Modifiers = mods;
            StartTime = Time.time; // Фиксируем время создания
        }
    }
}
