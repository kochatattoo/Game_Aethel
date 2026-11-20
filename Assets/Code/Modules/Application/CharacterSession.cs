using System;
using System.Collections.Generic;
using Code.Character.Data;
using CodeBase.Data;
using UniRx;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    public sealed class CharacterSession : ICharacterStatsSource, IDisposable
    {
        private readonly ICharacterDataService _hero;
        private readonly CompositeDisposable _statsBinding = new();
        private IReadOnlyDictionary<int, CharacterClass> _classes;
        private CharacterBalance _balance;
        private PlayerProgressValidator _validator;

        public event Action StatsChanged;

        public CharacterSession(ICharacterDataService hero) => _hero = hero;

        public void Configure(IReadOnlyDictionary<int, CharacterClass> classes, CharacterBalance balance)
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
                _hero.SetCharacter(null);
                return;
            }
            if (!IsValid(progress)) throw new InvalidOperationException("Сохранение персонажа несовместимо");

            var hero = new CharacterData(progress.Character.Restore(_classes[progress.Character.ClassId]), _balance);
            _hero.SetCharacter(hero);

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
