using System;
using System.IO;
using System.Linq;
using CodeBase.Enemies;
using PetProject.Encounter;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class EncounterCampaignSmoke
    {
        private const string ReportPath = "Library/Encounter/campaign-checks.json";

        [MenuItem("Tools/PetProject/Encounter/Start Isolated Run")]
        public static void Start() => CharacterMenuSmoke.Start();

        [MenuItem("Tools/PetProject/Encounter/Diagnose Installer")]
        public static void DiagnoseInstaller()
        {
            var installer = AssetDatabase.LoadAssetAtPath<CodeBase.DI.BootstrapInstaller>(
                "Assets/Prefabs/Resources/ProjectContext.prefab");
            if (installer == null) throw new InvalidOperationException("ProjectContext.prefab не найден");
            var serialized = new SerializedObject(installer);
            var fields = new[] { "_navigationConfig", "_encounterRules", "_encounterInput", "_heroPrefab", "_enemyPrefab" };
            var lines = new System.Collections.Generic.List<string>();
            foreach (var field in fields)
            {
                var property = serialized.FindProperty(field);
                lines.Add(field + "=" + (property == null ? "<missing>" : property.objectReferenceValue?.ToString() ?? "null"));
            }
            Directory.CreateDirectory("Library/Encounter");
            File.WriteAllLines("Library/Encounter/installer-diagnostic.txt", lines);
        }

        [MenuItem("Tools/PetProject/Encounter/Create Slot")]
        public static void CreateSlot()
        {
            var slots = Zenject.ProjectContext.Instance.Container.Resolve<CodeBase.Infrastructure.Services.SaveLoad.ISaveSlotService>();
            slots.PrepareNewProfile("slot-1");
            CharacterMenuSmoke.Creation();
        }

        [MenuItem("Tools/PetProject/Encounter/Validate Runtime")]
        public static void Validate()
        {
            var report = new Report();
            try
            {
                var hosts = Object.FindObjectsByType<EncounterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var host = hosts.FirstOrDefault(candidate => candidate.Ready) ?? hosts.FirstOrDefault();
                Require(host != null && host.Ready,
                    $"EncounterController не инициализирован: Error={host?.Error}; Map={host?.Map != null}; Hero={host?.HeroController != null}");
                Require(host.UseLegacyHud, "Не подключён campaign HUD");
                var hero = host.HeroController;
                Require(hero != null && hero.Instance != null, "HeroMotor не создан");
                Require(hero.Instance.GetComponent<CodeBase.Hero.HeroCombatStats>() != null,
                    "Не найден адаптер характеристик героя");
                var enemies = Object.FindObjectsByType<EnemyTarget>(FindObjectsSortMode.None);
                Require(enemies.Length > 0, "Противники не созданы через Encounter");
                var destination = hero.Instance.transform.position + Vector3.forward;
                report.moveAccepted = host.MoveNormally(destination);
                Require(report.moveAccepted, "Команда движения отклонена");
                report.scene = host.gameObject.scene.name;
                report.enemyCount = enemies.Length;
                report.success = true;
            }
            catch (Exception error)
            {
                report.error = error.ToString();
            }
            Directory.CreateDirectory("Library/Encounter");
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => CharacterMenuSmoke.Finish();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [Serializable]
        private sealed class Report
        {
            public bool success;
            public bool moveAccepted;
            public int enemyCount;
            public string scene;
            public string error;
        }
    }
}
