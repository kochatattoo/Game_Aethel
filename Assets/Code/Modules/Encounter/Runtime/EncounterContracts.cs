using System.Collections.Generic;
using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using CodeBase.Infrastructure.Utils;
using PetProject.Navigation;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;

namespace PetProject.Encounter
{
    public interface IEncounterContext
    {
        EncounterWorld World { get; }
        HeroMotor HeroController { get; }
        PointerInputProvider PointerInput { get; }
        IsometricCameraController CameraRig { get; }
        NavMeshGridMap Map { get; }
        WorldActionBudgets WorldActions { get; }
        GridPath Preview { get; }
        GameObject Hero { get; }
        GameObject EnemyPrefab { get; }
        MeleeTacticsAIConfig EnemyAI { get; }
        EncounterRulesConfig CombatRules { get; }
        NavMeshAgent HeroAgent { get; }
        HeroAnimator HeroAnimator { get; }
        Camera GameCamera { get; }
        IReadOnlyList<EnemySpawnDefinition> EnemySpawns { get; }
        int ActionBudget { get; }
        bool Combat { get; }
        bool Busy { get; }
        bool Ready { get; }
        bool PointerInputAllowed { get; }
        bool PointerOverUi { get; }
        bool MovePressed { get; }

        bool MoveNormally(Vector3 point);
        void PrepareForEncounter();
        void FinishEncounterAlignment();
        void RestoreExploration();
        void ResetHero();
        void SetNotice(string message);
        void AddHistory(string message);
        void RefreshTacticalView();
        void SetEncounterPreview(GridPath path, GridPosition cell);
        void ClearPreview();
    }
}
