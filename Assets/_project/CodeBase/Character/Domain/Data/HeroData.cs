using System;
using Code.Hero.Items;
namespace Code.Hero.Data
{
    public sealed class HeroData : IDisposable
    {
        public HeroClass Hero { get; }
        public HeroEquipmentService Equipment { get; }=new();
        public HeroBuffService Buffs { get; }=new();
        public HeroDerivedStats Stats { get; }
        public HeroData(HeroClass hero,CharacterBalance balance=null){Hero=hero;Stats=new HeroDerivedStats(hero,balance ?? new CharacterBalance());}
        public void Dispose(){Buffs.Dispose();Equipment.Dispose();Stats.Dispose();Hero.Dispose();}
    }
}

