using System;
using System.Collections.Generic;
using PetProject.Encounter;
using PetProject.Navigation;
using PetProject.Tactics;
using UnityEngine;

namespace CodeBase.UI.Encounter
{
    public interface ITacticalGridContext
    {
        NavigationConfig Config { get; }
        NavMeshGridMap Map { get; }
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

    public interface IEncounterCombatContext
    {
        EncounterSnapshot Snapshot { get; }
        bool CanReset { get; }
        bool IsDefeated { get; }
        bool PlayerTurnReady { get; }
        int HeroHealth { get; }
        int HeroMaxHealth { get; }
        int EnemyHealth { get; }
        int EnemyMaxHealth { get; }
        bool IsEnemySelected { get; }
        string SelectedEnemyName { get; }
        void ResetCombat();
        void EndTurnFromHud();
    }

    public interface IEncounterHudContext
    {
        WorldTurnClock WorldTurn { get; }
        WorldActionBudgets WorldActions { get; }
        TacticalMovementSession Session { get; }
        IEncounterCombatContext EncounterDirector { get; }
        IEncounterRules CombatRules { get; }
        string Status { get; }
        string Notice { get; }
        int ActionBudget { get; }
        bool Combat { get; }
        bool EncounterActive { get; }
        bool Ready { get; }
        bool Busy { get; }
        bool CanSwitchMode { get; }
        IReadOnlyList<string> History { get; }
        event Action Changed;
        bool SetCombat(bool requested);
        void NewTurn();
    }
}
