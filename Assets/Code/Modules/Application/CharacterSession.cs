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
        private PlayerProgressValidator _validator;

        public event Action StatsChanged;

        public CharacterSession(IHeroDataService hero) => _hero = hero;

        public void Configure(IReadOnlyDictionary<int, HeroClass> classes, CharacterBalance balance)
        {
            _classes = classes;
            _balance = balance;
            _validator = new PlayerProgressValidator(classes);
        }

        public bool IsValid(PlayerProgress progress) => _validator.IsValid(progress);

        public void RestoreProfile(PlayerProgress progress)
        {
            _statsBinding.Clear();
            if (progress.Character == null)
            {
                _hero.SetHero(null);
                return;
            }
            if (!IsValid(progress)) throw new InvalidOperationException("Сохранение персонажа несовместимо");

            var hero = new HeroData(progress.Character.Restore(_classes[progress.Character.ClassId]), _balance);
            _hero.SetHero(hero);
            hero.Stats.Damage.Subscribe(value =>
            {
                progress.HeroStats.Damage = value;
                StatsChanged?.Invoke();
            }).AddTo(_statsBinding);
            hero.Stats.MaxHealth.Subscribe(value =>
            {
                progress.HeroState.MaxHP = value;
                progress.HeroState.CurrentHP = Mathf.Clamp(progress.HeroState.CurrentHP, 0, value);
                StatsChanged?.Invoke();
            }).AddTo(_statsBinding);
        }

        public void Dispose() => _statsBinding.Dispose();
    }
}
