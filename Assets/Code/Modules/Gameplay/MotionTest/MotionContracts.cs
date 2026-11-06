using System;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.MotionTest
{
    public interface IMotionCameraInput
    {
        MotionPointerInput MotionInput { get; }
        bool PointerInputAllowed { get; }
        bool KeyboardInputAllowed { get; }
        bool PointerOverUi { get; }
    }

    public interface IMotionGridContext
    {
        MotionTestWorld World { get; }
        SurfaceGridMap Map { get; }
        GameObject Hero { get; }
        GridPath Preview { get; }
        GridPosition? Hover { get; }
        GridPosition TacticalPosition { get; }
        long TacticalRevision { get; }
        int TacticalActions { get; }
        bool Ready { get; }
        bool TacticalActive { get; }
        bool ShowAllCells { get; }
        bool Busy { get; }
    }

    public interface IMotionHudContext
    {
        WorldTurnClock WorldTurn { get; }
        WorldActionBudgets WorldActions { get; }
        MotionTestSession Session { get; }
        SkeletonCombatDirector EncounterDirector { get; }
        string Status { get; }
        string Notice { get; }
        int TestBudget { get; }
        bool Combat { get; }
        bool EncounterActive { get; }
        bool Ready { get; }
        bool Busy { get; }
        bool CanSwitchMode { get; }
        event Action Changed;

        bool SetCombat(bool requested);
        void NewTurn();
    }

    public interface IMotionEncounterContext
    {
        MotionTestWorld World { get; }
        MotionHero HeroController { get; }
        MotionPointerInput MotionInput { get; }
        MotionCameraController CameraRig { get; }
        SurfaceGridMap Map { get; }
        WorldActionBudgets WorldActions { get; }
        GridPath Preview { get; }
        GameObject Hero { get; }
        GameObject EnemyPrefab { get; }
        MeleeTacticsAIConfig EnemyAI { get; }
        NavMeshAgent HeroAgent { get; }
        HeroAnimator HeroAnimator { get; }
        Camera GameCamera { get; }
        Vector3 EnemySpawn { get; }
        int TestBudget { get; }
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
        void RefreshTacticalView();
        void SetEncounterPreview(GridPath path, GridPosition cell);
        void ClearPreview();
    }
}
