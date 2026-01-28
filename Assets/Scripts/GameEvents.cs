using System;
using UnityEngine;

public static class GameEvents
{
    // Event triggered when Theseus makes a move
    public static event Action OnTheseusMoved;

    // Event triggered when Theseus waits
    public static event Action OnTheseusWaited;

    // Event triggered when game is won
    public static event Action OnGameWon;

    // Event triggered when game is lost
    public static event Action OnGameLost;

    // Event triggered when game starts
    public static event Action OnGameStarted;

    // Event triggered when a level is loaded
    public static event Action<int> OnLevelLoaded;

    //Event triggered when undo is requested.
    public static event Action OnUndoRequested;
    
    //Event triggered when a turn has been completed.
    public static event Action OnTurnCompleted;
    
    //Event triggered for commentary UI.
    public static event Action<string> OnNewLevelLoad;

    // Methods to invoke events
    public static void TheseusMoved()
    {
        OnTheseusMoved?.Invoke();
    }

    public static void TheseusWaited()
    {
        OnTheseusWaited?.Invoke();
    }

    public static void GameWon()
    {
        OnGameWon?.Invoke();
    }

    public static void GameLost()
    {
        OnGameLost?.Invoke();
    }

    public static void GameStarted()
    {
        OnGameStarted?.Invoke();
    }

    public static void LevelLoaded(int levelIndex)
    {
        OnLevelLoaded?.Invoke(levelIndex);
    }

    public static void UndoRequested()
    {
        OnUndoRequested?.Invoke();
    }
    
    public static void TurnCompleted()
    {
        OnTurnCompleted?.Invoke();
    }

    public static void NewLevelLoaded(string text)
    {
        OnNewLevelLoad?.Invoke(text);
    }
}