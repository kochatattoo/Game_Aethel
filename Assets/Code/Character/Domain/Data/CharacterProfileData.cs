using System;
using System.Collections.Generic;
namespace Code.Hero.Data
{
    [Serializable]
    public sealed class CharacterProfileData
    {
        public int ClassId;
        public List<AttributeEntry> Attributes=new();
        public int ReferenceStrength;
        public int ReferenceStamina;
        public static CharacterProfileData Capture(HeroClass hero)
        {
            var data=new CharacterProfileData{ClassId=hero.Id,ReferenceStrength=hero.ReferenceStrength,ReferenceStamina=hero.ReferenceStamina};
            foreach(var pair in hero.Attributes.AllAttributes)data.Attributes.Add(new AttributeEntry{key=pair.Key,value=pair.Value.BaseValue});
            return data;
        }
        public HeroClass Restore(HeroClass definition)
        {
            if(definition.Id!=ClassId || Attributes==null)throw new ArgumentException("Неизвестный класс героя");
            var values=new Dictionary<string,int>();foreach(var item in Attributes){if(item==null||item.value<0||!values.TryAdd(item.key,item.value))throw new ArgumentException("Неверные атрибуты сохранения");}
            if(values.Count!=5)throw new ArgumentException("Неверное число атрибутов");
            return new HeroClass(definition.Id,definition.Name,values,definition.Color,definition.Description,definition.ImagePath,definition.Abilities,ReferenceStrength,ReferenceStamina);
        }
    }
}

