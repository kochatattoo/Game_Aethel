using System;
using System.Collections.Generic;
using Code.Hero.Data;
using CodeBase.Data;
using UniRx;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    public sealed class CharacterSession : ICharacterStatsSource, IDisposable
    {
        private readonly IHeroDataService _hero;
        private readonly CompositeDisposable _statsBinding = new();
        private IReadOnlyDictionary<int, HeroClass> _classes;
        private CharacterBalance _balance;
        public event Action StatsChanged;
        public CharacterSession(IHeroDataService hero) => _hero = hero;
        public void Configure(IReadOnlyDictionary<int, HeroClass> classes, CharacterBalance balance)
        {
            _classes = classes;
            _balance = balance;
        }
        public bool IsValid(PlayerProgress data)
        {
            if(data?.HeroState==null || data.HeroStats==null || data.WorldData?.PositionOnLevel==null ||
               data.WorldData.LootData==null || data.KillData==null || data.PurchaseData==null ||
               !Application.CanStreamedLevelBeLoaded(data.WorldData.PositionOnLevel.Level) ||
               !Finite(data.HeroState.CurrentHP) || !Finite(data.HeroState.MaxHP) || data.HeroState.MaxHP<=0 ||
               data.HeroState.CurrentHP<0 || data.HeroState.CurrentHP>data.HeroState.MaxHP ||
               !Finite(data.HeroStats.Damage) || data.HeroStats.Damage<0 ||
               data.WorldData.LootData.Collected<0 || data.KillData.ClaeredSpawners==null ||
               data.WorldData.LootData.LootsOnGround?.Dict==null || data.PurchaseData.BoughtIAPs?.Dict==null)
                return false;
            if(data.Character==null)return true;
            if(data.CharacterSchemaVersion!=1 || !_classes.TryGetValue(data.Character.ClassId,out var definition))return false;
            try{using var restored=data.Character.Restore(definition);return true;}catch(ArgumentException){return false;}
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);

        public void RestoreProfile(PlayerProgress progress)
        {
            _statsBinding.Clear();
            if(progress.Character==null){_hero.SetHero(null);return;}
            if(!IsValid(progress))throw new InvalidOperationException("Сохранение персонажа несовместимо");
            var hero=new HeroData(progress.Character.Restore(_classes[progress.Character.ClassId]),_balance);
            _hero.SetHero(hero);
            hero.Stats.Damage.Subscribe(v=>{progress.HeroStats.Damage=v;StatsChanged?.Invoke();}).AddTo(_statsBinding);
            hero.Stats.MaxHealth.Subscribe(v=>{progress.HeroState.MaxHP=v;progress.HeroState.CurrentHP=Mathf.Clamp(progress.HeroState.CurrentHP,0,v);StatsChanged?.Invoke();}).AddTo(_statsBinding);
        }


        public void Dispose() => _statsBinding.Dispose();
    }
}
