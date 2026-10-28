using System.Collections.Generic;
using System.Threading;
using Code.Hero.Attributes;
using Code.Hero.Data;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Attribute = Code.Hero.Attributes.Attribute;

namespace PetProject.Character.Tests
{
    [Author("PetProject"), Category("CharacterMigration")]
    public sealed class CharacterMigrationTests
    {
        private static HeroClass Definition()=>new(1,"Воин",new Dictionary<string,int>{{"str",12},{"agi",10},{"inte",10},{"wis",8},{"stm",12}},"#ffffff","Описание","icon",new List<string>());
        private sealed class Provider : IHeroDataProvider
        {
            public IReadOnlyDictionary<int,HeroClass> HeroClassesReadonly {get;}
            public Provider(HeroClass hero)=>HeroClassesReadonly=new Dictionary<int,HeroClass>{{hero.Id,hero}};
            public UniTask LoadAsync(CancellationToken token)=>UniTask.CompletedTask;
        }
        [Test] public void DonorPercentagePolicyIsPreserved()
        {
            using var stat=new Attribute(100);stat.AddModifier(new AttributeModifier(ModifierType.Percantage,20));stat.AddModifier(new AttributeModifier(ModifierType.Percantage,-20));Assert.That(stat.FinalValue.Value,Is.EqualTo(96));
        }
        [Test] public void ReplacingSourceDoesNotStackOrRemoveAnotherSource()
        {
            using var stat=new Attribute(10);stat.AddModifier(new AttributeModifier(ModifierType.Flat,3,"buff"));
            for(var i=0;i<50;i++)stat.ReplaceSource("weapon",new[]{new AttributeModifier(ModifierType.Flat,2,"weapon")});
            Assert.That(stat.FinalValue.Value,Is.EqualTo(15));Assert.That(stat.ModifierCount,Is.EqualTo(2));stat.RemoveSource("weapon");Assert.That(stat.FinalValue.Value,Is.EqualTo(13));
        }
        [Test] public void CancelledDraftDoesNotMutateCatalog()
        {
            using var definition=Definition();using(var draft=new HeroDataFabric(new Provider(definition))){draft.Initialize();draft.SelectClass(1);draft.TryAllocate("str",1);Assert.That(definition.Attributes.GetValue("str"),Is.EqualTo(12));}
            Assert.That(definition.Attributes.GetValue("str"),Is.EqualTo(12));
        }
        [Test] public void PlusMinusDoesNotAccumulateModifiersOrPoints()
        {
            using var definition=Definition();using var draft=new HeroDataFabric(new Provider(definition));draft.Initialize();draft.SelectClass(1);
            for(var i=0;i<50;i++){Assert.That(draft.TryAllocate("str",1),Is.True);Assert.That(draft.TryAllocate("str",-1),Is.True);}
            Assert.That(draft.BonusPoints.Value,Is.EqualTo(10));Assert.That(draft.SelectedClass.Value.Attributes.AllAttributes["str"].ModifierCount,Is.Zero);Assert.That(draft.TryAllocate("str",-1),Is.False);
        }
        [Test] public void DraftCannotOverspendAndCommittedHeroSurvivesDraftDisposal()
        {
            using var definition=Definition();HeroData committed;
            using(var draft=new HeroDataFabric(new Provider(definition))){draft.Initialize();draft.SelectClass(1);for(var i=0;i<10;i++)draft.TryAllocate("str",1);Assert.That(draft.TryAllocate("str",1),Is.False);committed=draft.CreateHeroData();}
            using(committed){Assert.That(committed.Hero.Attributes.GetValue("str"),Is.EqualTo(22));Assert.That(committed.Stats.Damage.Value,Is.EqualTo(7));}
        }
        [Test] public void ProfileRoundTripRestoresBaseWithoutTransientBuffs()
        {
            using var original=Definition();original.Attributes.AddBaseValue("str",2);original.Attributes.AddModifier("str",new AttributeModifier(ModifierType.Flat,5,"buff"));
            var snapshot=JsonUtility.FromJson<CharacterProfileData>(JsonUtility.ToJson(CharacterProfileData.Capture(original)));
            using var restored=snapshot.Restore(original);Assert.That(restored.Attributes.GetValue("str"),Is.EqualTo(14));Assert.That(restored.ReferenceStrength,Is.EqualTo(12));
        }
        [Test] public void DerivedStatsPreserveBaselineAndReactToAttributes()
        {
            using var hero=new HeroData(Definition());Assert.That(hero.Stats.MaxHealth.Value,Is.EqualTo(50));Assert.That(hero.Stats.Damage.Value,Is.EqualTo(2));
            hero.Hero.Attributes.AddBaseValue("stm",1);hero.Hero.Attributes.AddBaseValue("str",2);Assert.That(hero.Stats.MaxHealth.Value,Is.EqualTo(55));Assert.That(hero.Stats.Damage.Value,Is.EqualTo(3));
        }
        [Test] public void IncompleteProfileIsRejected()
        {
            using var definition=Definition();var data=new CharacterProfileData{ClassId=1};Assert.Throws<System.ArgumentException>(()=>data.Restore(definition));
        }
    }
}
