using System;
using System.Collections.Generic;
using Code.Hero.Data;
using CodeBase.Data;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    internal sealed class PlayerProgressValidator
    {
        private readonly IReadOnlyDictionary<int, HeroClass> _classes;

        public PlayerProgressValidator(IReadOnlyDictionary<int, HeroClass> classes)
        {
            _classes = classes;
        }

        public bool IsValid(PlayerProgress progress)
        {
            if (!HasValidWorld(progress) || !HasValidStats(progress)) return false;
            if (progress.Character == null) return true;
            if (progress.CharacterSchemaVersion != 1 ||
                !_classes.TryGetValue(progress.Character.ClassId, out var definition)) return false;
            try
            {
                using var restored = progress.Character.Restore(definition);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool HasValidWorld(PlayerProgress progress) =>
            progress?.HeroState != null && progress.HeroStats != null &&
            progress.WorldData?.PositionOnLevel != null && progress.WorldData.LootData != null &&
            progress.KillData != null && progress.PurchaseData != null &&
            Application.CanStreamedLevelBeLoaded(progress.WorldData.PositionOnLevel.Level) &&
            progress.WorldData.LootData.Collected >= 0 && progress.KillData.ClaeredSpawners != null &&
            progress.WorldData.LootData.LootsOnGround?.Dict != null && progress.PurchaseData.BoughtIAPs?.Dict != null;

        private static bool HasValidStats(PlayerProgress progress) =>
            IsFinite(progress.HeroState.CurrentHP) && IsFinite(progress.HeroState.MaxHP) &&
            progress.HeroState.MaxHP > 0 && progress.HeroState.CurrentHP >= 0 &&
            progress.HeroState.CurrentHP <= progress.HeroState.MaxHP &&
            IsFinite(progress.HeroStats.Damage) && progress.HeroStats.Damage >= 0;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
