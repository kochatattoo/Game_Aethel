using System;
using System.Collections.Generic;
using Code.Character.Attributes;
using Code.Character.Data;
using UniRx;
namespace Code.Items
{
    public sealed class BuffService : IDisposable
    {
        public ReactiveCollection<ActiveBuff> ActiveBuffs { get; }=new();
        public void ApplyBuff(CharacterClass hero,ConsumableItem item)
        {
            var source="buff:"+Guid.NewGuid().ToString("N");var modifiers=new List<AttributeModifier>();
            foreach(var data in item.Modifiers){var m=new AttributeModifier(data.Type,data.Amount,source);hero.Attributes.AddModifier(data.GetKey(),m);modifiers.Add(m);}
            var buff=new ActiveBuff(item,modifiers);ActiveBuffs.Add(buff);
            Observable.Timer(TimeSpan.FromSeconds(item.Duration),Scheduler.MainThread).Subscribe(_=>Remove(hero,buff,source)).AddTo(buff.Disposables);
        }
        private void Remove(CharacterClass hero,ActiveBuff buff,string source){foreach(var p in hero.Attributes.AllAttributes.Values)p.RemoveSource(source);ActiveBuffs.Remove(buff);buff.Disposables.Dispose();}
        public void Dispose(){foreach(var buff in ActiveBuffs)buff.Disposables.Dispose();ActiveBuffs.Clear();ActiveBuffs.Dispose();}
    }
}

