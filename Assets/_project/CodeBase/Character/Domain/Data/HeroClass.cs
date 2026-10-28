using System;
using System.Collections.Generic;
using Code.Hero.Attributes;
namespace Code.Hero.Data
{
    public sealed class HeroClass : IDisposable
    {
        public int Id { get; }
        public string Name { get; }
        public HeroAttributes Attributes { get; }
        public string Color { get; }
        public string Description { get; }
        public string ImagePath { get; }
        public List<string> Abilities { get; }
        public int ReferenceStrength { get; }
        public int ReferenceStamina { get; }
        public HeroClass(HeroClassDTO dto) : this(dto.id,dto.name,Read(dto.attributes),dto.color,dto.description,dto.imagePath,dto.abilities) {}
        public HeroClass(int id,string name,Dictionary<string,int> values,string color,string description,string imagePath,List<string> abilities,int referenceStrength=-1,int referenceStamina=-1)
        {
            foreach(AttributeName attr in Enum.GetValues(typeof(AttributeName))) if(!values.ContainsKey(attr.ToStringValue())) throw new ArgumentException("Не все атрибуты заданы");
            Id=id;Name=name;Color=color;Description=description;ImagePath=imagePath;Abilities=new List<string>(abilities ?? new List<string>());
            Attributes=new HeroAttributes(new Dictionary<string,int>(values));
            ReferenceStrength=referenceStrength<0?values[AttributeConstants.STR]:referenceStrength;
            ReferenceStamina=referenceStamina<0?values[AttributeConstants.STM]:referenceStamina;
        }
        private static Dictionary<string,int> Read(List<AttributeEntry> entries)
        {
            var values=new Dictionary<string,int>();
            if(entries==null) throw new ArgumentException("Пустые атрибуты класса");
            foreach(var entry in entries) if(entry==null || string.IsNullOrWhiteSpace(entry.key) || entry.value<0 || !values.TryAdd(entry.key,entry.value)) throw new ArgumentException("Некорректные атрибуты класса");
            return values;
        }
        public HeroClass Clone(bool includeModifiers=false)
        {
            var values=new Dictionary<string,int>();
            foreach(var p in Attributes.AllAttributes) values.Add(p.Key,includeModifiers?p.Value.FinalValue.Value:p.Value.BaseValue);
            return new HeroClass(Id,Name,values,Color,Description,ImagePath,Abilities,ReferenceStrength,ReferenceStamina);
        }
        public void ResetAllAttributes() { foreach(var attr in Attributes.AllAttributes.Values) attr.ResetModifiers(); }
        public void Dispose()=>Attributes.Dispose();
    }
}

