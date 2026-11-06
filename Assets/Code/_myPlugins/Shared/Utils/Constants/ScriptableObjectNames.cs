using System;

namespace Shared.Utils.Constants
{
    public static class ScriptableObjectNames
    {
        private const string MenuName = "Configs";
        
        public const string DiCapabilityMenu = MenuName + "/Person/Di/";
        public const string DiCapabilityConfigsCommonMenu = DiCapabilityMenu + "Configs/Common/";
        public const string DiCapabilityConfigsPlayerMenu = DiCapabilityMenu + "Configs/Player/";
        public const string DiCapabilityConfigsEnemyMenu = DiCapabilityMenu + "Configs/Enemy/";

        public const string PersonCommonSettings = MenuName + "/Person/";

        public const string SceneLoadingMenu = MenuName + "/Scenes/";
        public const string EffectsMenu = MenuName + "/Effects/";
        public const string LootMenu = MenuName + "/Loot/";
        public const string AnimatorMenu = MenuName + "/Person/Animator/";
        public const string AudioMenu = MenuName + "/Audio/";
        public const string AudioAssets = AudioMenu + "Assets/";
        public const string AudioMaping = AudioMenu + "Maping/";
        public const string AudioSettings = AudioMenu + "Settings/";
        public const string CombatMenu = MenuName + "/Combat/";
        public const string VFXName = MenuName + "/VFX/";
        public const string VFXSettings = VFXName + "Setting/";
        public const string VFXMapping = VFXName + "Mapping/";
        public const string TeasName = MenuName + "/TEAS/";
        public const string GOAPName = PersonCommonSettings + "/Enemy/";
        
        // Все константы, помеченные Obsolete - убрать и использовать вместо этого ИмяМеню + nameof(class)
        
        #region Filenames
        [Obsolete]
        public const string ScheduleName = "Schedule";
        [Obsolete]
        public const string TaskSettingsName = "TaskSettings";
        [Obsolete]
        public const string BodyPartSettingsName = "BodyPartSettings";
        [Obsolete]
        public const string CameraSettingsName = "CameraSettings";
        [Obsolete]
        public const string EffectSettingsName = "EffectSettings";
        [Obsolete]
        public const string EnemyFOVSettingsName = "EnemyFOVSettings";
        [Obsolete]
        public const string PersonConfigName = "PersonConfig";
        [Obsolete]
        public const string TyingSettingsName = "TyingSettings";
        [Obsolete]
        public const string TimeRangeSettingsName = "TimeRangeSettings";
        [Obsolete]
        public const string AlarmSettingsName = "AlarmSettings";
        [Obsolete]
        public const string InteractableSettingsName = "InteractableSettings";
        [Obsolete]
        public const string AnxietySettingsName = "AnxietySettings";
        [Obsolete]
        public const string LoadSceneSettingName = "LoadSceneSetting";
        [Obsolete]
        public const string EffectUISettingsName = "EffectUISettings";
        [Obsolete]
        public const string PetalCombatSettingsName = "PetalCombatSettings";
        [Obsolete]
        public const string NoiseSettingsName = "NoiseSettings";
        [Obsolete]
        public const string PlayerSettingsName = "PlayerSettings";
        [Obsolete]
        public const string EnemySettingsName = "EnemySettings";
        [Obsolete]
        public const string EnemyConfigName = "EnemyConfig";
        [Obsolete]
        public const string PatrolPointBehaviorName = "PatrolPointBehavior";
        [Obsolete]
        public const string InventorySettingsName = "InventorySettings";
        [Obsolete]
        public const string ItemDataName = "ItemData";
        [Obsolete]
        public const string ArmorParametersName = "ArmorParameters";
        [Obsolete]
        public const string MeleeParametersName = "MeleeParameters";
        [Obsolete]
        public const string FistParametersName = "FistParameters";
        [Obsolete]
        public const string GlovesParametersName = "GlovesParameters";
        [Obsolete]
        public const string RangedWeaponParametersName = "RangeParameters";
        [Obsolete]
        public const string RangedAmmunitionParametersName = "RangedAmmunitionParameters";
        [Obsolete]
        public const string TorchParametersName = "TorchParameters";
        [Obsolete]
        public const string TargetSwitcherSettingsName = "TargetSwitcherSettings";
        [Obsolete]
        public const string AnimatorLocomotionSettingsName = "AnimatorLocomotionSettings";
        [Obsolete]
        public const string AnimatorActionSettingsName = "AnimatorActionSettings";
        [Obsolete]
        public const string ClimbDetectionSettingsName = "ClimbDetectionSettings";
        [Obsolete]
        public const string HopDetectionSettingsName = "HopDetectionSettings";
        [Obsolete]
        public const string ObstacleDetectionSettingsName = "ObstacleDetectionSettings";
        [Obsolete]
        public const string EnemyGroupSettingsName = "EnemyGroupSettings";
        [Obsolete]
        public const string VisibilityDetectionSettingsName = "VisibilityDetectionSettings";
        [Obsolete]
        public const string ConsumptionDiseaseSettingsName = "ConsumptionSettings";
        [Obsolete]
        public const string InflammationDiseaseSettingsName = "InflammationSettings";
        [Obsolete]
        public const string MigraineDiseaseSettingsName = "MigraineSettings";
        [Obsolete]
        public const string DiseasesConfigName = "DiseasesConfig";
        [Obsolete]
        public const string CameraRelativeDetectionSettingsName = "CameraRelativeDetectionSettings";
        [Obsolete]
        public const string ObstacleSettingsName = "HopSettings";
        [Obsolete]
        public const string AnimatorSettingsName = "AnimatorSettings";
        [Obsolete]
        public const string StunSettingsName = "StunSettings";
        [Obsolete]
        public const string CommonWindowsSettingsName = "Common Windows Settings";
        [Obsolete]
        public const string HudWindowsSettingsName = "HudWindowSettings";
        [Obsolete]
        public const string GlobalTimeSettingsName = "GlobalTimeSettings";
        
        [Obsolete]
        public const string AnimatorNamesGroupConfigName = "AnimatorNamesGroupConfig";
        [Obsolete]
        public const string AttackAnimatorNamesGroupConfigName = "AttackAnimatorNamesGroupConfig";
        [Obsolete]
        public const string HitReactAnimatorNamesGroupConfigName = "HitReactAnimatorNamesGroupConfig";
        [Obsolete]
        public const string BounceAnimatorNamesGroupConfigName = "BounceAnimatorNamesGroupConfig";
        [Obsolete]
        public const string ReadyStowAnimatorNamesGroupConfigName = "ReadyStowAnimatorNamesGroupConfig";
        [Obsolete]
        public const string BlockHandleAnimatorNamesGroupConfigName = "BlockHandleAnimatorNamesGroupConfig";
        [Obsolete]
        public const string DodgeAnimatorNamesGroupConfigName = "DodgeAnimatorNamesGroupConfig";
        [Obsolete]
        public const string AnimatorOverrideGroupConfigName = "AnimatorOverrideGroupConfig";
        [Obsolete]
        public const string ClimbHopAnimatorNamesGroupConfigName = "ClimbAnimatorNamesGroupConfig";
        [Obsolete]
        public const string ExecuteLogicAnimatorNamesGroupConfigName = "ExecuteLogicAnimatorNamesGroupConfig";
        [Obsolete]
        public const string AlertAnimatorNamesGroupConfigName = "AlertAnimatorNamesGroupConfig";
        [Obsolete]
        public const string RestAnimatorNamesGroupConfigName = "RestAnimatorNamesGroupConfig";

        [Obsolete]
        public const string PersonAnimatorBundleName = "PersonAnimatorBundle";
        [Obsolete]
        public const string PlayerAnimatorAddonBundleName = "PlayerAnimatorAddonBundle";
        [Obsolete]
        public const string EnemyAnimatorAddonBundleName = "EnemyAnimatorAddonBundle";
        
        #endregion
        
        #region Path names
        [Obsolete]
        public const string PersonMenuName = MenuName + "/Person/";
        [Obsolete]
        public const string ScheduleMenuName = MenuName + "/Person/Enemy/" + ScheduleName;
        [Obsolete]
        public const string TaskSettingsMenuName = MenuName + "/Person/Enemy/" + TaskSettingsName;
        [Obsolete]
        public const string BodyPartSettingsMenuName = MenuName + "/Person/" + BodyPartSettingsName;
        [Obsolete]
        public const string CameraSettingsMenuName = MenuName + "/Services/" + CameraSettingsName;
        [Obsolete]
        public const string EffectSettingsMenuName = MenuName + "/Person/" + EffectSettingsName;
        [Obsolete]
        public const string EnemyFOVSettingsMenuName = MenuName + "/Person/Enemy/" + EnemyFOVSettingsName;
        [Obsolete]
        public const string PersonConfigMenuName = MenuName + "/Person/" + PersonConfigName;
        [Obsolete]
        public const string TyingSettingsMenuName = MenuName + "/Person/" + TyingSettingsName;
        [Obsolete]
        public const string TimeRangeSettingsMenuName = MenuName + "/Services/" + TimeRangeSettingsName;
        [Obsolete]
        public const string AlarmSettingsMenuName = MenuName + "/Person/Enemy/" + AlarmSettingsName;
        [Obsolete]
        public const string InteractableSettingsMenuName = MenuName + "/Person/" + InteractableSettingsName;
        [Obsolete]
        public const string AnxietySettingsMenuName = MenuName + "/Person/Enemy/" + AnxietySettingsName;
        [Obsolete]
        public const string LoadSceneSettingNameMenuName = MenuName + "/System/" + LoadSceneSettingName;
        [Obsolete]
        public const string EffectMenuName = MenuName + "/Effects/";
        [Obsolete]
        public const string EffectUISettingsMenuName = EffectMenuName + EffectUISettingsName;
        [Obsolete]
        public const string PetalCombatSettingsMenuName = MenuName + "/Person/Player/" + PetalCombatSettingsName;
        [Obsolete]
        public const string NoiseSettingsMenuName = MenuName + "/Person/Player/" + NoiseSettingsName;
        [Obsolete]
        public const string PlayerSettingsMenuName = MenuName + "/Person/Player/" + PlayerSettingsName;
        [Obsolete]
        public const string EnemySettingsMenuName = MenuName + "/Person/Enemy/" + EnemySettingsName;
        [Obsolete]
        public const string EnemyConfigMenuName = MenuName + "/Person/Enemy/" + EnemyConfigName;
        [Obsolete]
        public const string PatrolPointBehaviorMenuName = MenuName + "/Person/Enemy/" + PatrolPointBehaviorName;
        [Obsolete]
        public const string InventorySettingsMenuName = MenuName + "/InventorySystem/" + InventorySettingsName;
        [Obsolete]
        public const string ItemDataMenuName = MenuName + "/Items/" + ItemDataName;
        [Obsolete]
        public const string ParametersMenuName = MenuName + "/Items/Parameters/";
        [Obsolete]
        public const string ArmorParametersMenuName = ParametersMenuName + ArmorParametersName;
        [Obsolete]
        public const string MeleeParametersMenuName = ParametersMenuName + MeleeParametersName;
        [Obsolete]
        public const string RangedWeaponParametersMenuName = ParametersMenuName + "/Items/Parameters/" + RangedWeaponParametersName;
        [Obsolete]
        public const string RangedAmmunitionParametersMenuName = ParametersMenuName + "/Items/Parameters/" + RangedAmmunitionParametersName;
        [Obsolete]
        public const string TorchParametersMenuName = ParametersMenuName + TorchParametersName;
        [Obsolete]
        public const string FistParametersMenuName = ParametersMenuName + FistParametersName;
        [Obsolete]
        public const string GlovesParametersMenuName = ParametersMenuName + GlovesParametersName;
        [Obsolete]
        public const string TargetSwitcherSettingsMenuName = MenuName + "/Services/" + TargetSwitcherSettingsName;
        [Obsolete]
        public const string AnimatorLocomotionSettingsMenuName = MenuName + "/Person/Animator/" + AnimatorLocomotionSettingsName;
        [Obsolete]
        public const string AnimatorActionSettingsMenuName = MenuName + "/Person/Animator/" + AnimatorActionSettingsName;
        [Obsolete]
        public const string ClimbDetectionSettingsMenuName = MenuName + "/Person/Player/Climb/" + ClimbDetectionSettingsName;
        [Obsolete]
        public const string HopDetectionSettingsMenuName = MenuName + "/Person/Player/Hop/" + HopDetectionSettingsName;
        [Obsolete]
        public const string ObstacleDetectionSettingsMenuName = MenuName + "/Person/Player/Obstacle/" + ObstacleDetectionSettingsName;
        [Obsolete]
        public const string EnemyGroupSettingsMenuName = MenuName + "/Person/Enemy/" + EnemyGroupSettingsName;
        [Obsolete]
        public const string VisibilityDetectionSettingsMenuName = MenuName + "/Person/" + VisibilityDetectionSettingsName;
        [Obsolete]
        public const string ConsumptionDiseaseSettingsMenuName = MenuName + "/Person/Diseases/" + ConsumptionDiseaseSettingsName;
        [Obsolete]
        public const string InflammationDiseaseSettingsMenuName = MenuName + "/Person/Diseases/" + InflammationDiseaseSettingsName;
        [Obsolete]
        public const string MigraineDiseaseSettingsMenuName = MenuName + "/Person/Diseases/" + MigraineDiseaseSettingsName;
        [Obsolete]
        public const string DiseasesConfigMenuName = MenuName + "/Person/" + DiseasesConfigName;
        [Obsolete]
        public const string ObstacleSettingsMenuName = MenuName + "/Game/Movement/" + ObstacleSettingsName;
        [Obsolete]
        public const string AnimatorSettingsMenuName = MenuName + "/Person/Animator/" + ObstacleSettingsName;
        [Obsolete]
        public const string StunSettingsMenuName = MenuName + "/Person/" + StunSettingsName;
        [Obsolete]
        public const string GlobalTimeSettingsMenuName = MenuName + "/Services/" + GlobalTimeSettingsName;
        
        [Obsolete]
        public const string AnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + AnimatorNamesGroupConfigName;
        [Obsolete]
        public const string AttackAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + AttackAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string HitReactAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + HitReactAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string BounceAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + BounceAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string ReadyStowAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + ReadyStowAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string BlockHandleAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + BlockHandleAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string DodgeAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + DodgeAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string AnimatorOverrideGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + AnimatorOverrideGroupConfigName;
        [Obsolete]
        public const string ClimbHopAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + ClimbHopAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string ExecuteLogicAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + ExecuteLogicAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string AlertAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + AlertAnimatorNamesGroupConfigName;
        [Obsolete]
        public const string RestAnimatorNamesGroupConfigMenuName = MenuName + "/Person/Animator/Groups/" + RestAnimatorNamesGroupConfigName;

        [Obsolete]
        public const string CameraRelativeDetectionSettingsMenuName = MenuName + "/Services/" + CameraRelativeDetectionSettingsName;
        
        [Obsolete]
        public const string CommonWindowsSettingsMenuName = MenuName + "/Windows/" + CommonWindowsSettingsName;
        [Obsolete]
        public const string HudWindowsSettingsMenuName = MenuName + "/Windows/" + HudWindowsSettingsName;
        [Obsolete]
        public const string PersonAnimatorBundleMenuName = MenuName + "/Person/Animator/Bundles" + PersonAnimatorBundleName;
        [Obsolete]
        public const string PlayerAnimatorAddonBundleMenuName = MenuName + "/Person/Animator/Bundles" + PlayerAnimatorAddonBundleName;
        [Obsolete]
        public const string EnemyAnimatorAddonBundleMenuName = MenuName + "/Person/Animator/Bundles" + EnemyAnimatorAddonBundleName;
        #endregion
    }
}
