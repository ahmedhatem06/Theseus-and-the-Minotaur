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
    
    // Event triggered when level needs to restart
    public static event Action OnLevelRestart;
    
    // Event triggered when game starts
    public static event Action OnGameStarted;
    
    // Event triggered when a level is loaded
    public static event Action<int> OnLevelLoaded;
    
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
    
    public static void RestartLevel()
    {
        OnLevelRestart?.Invoke();
    }
    
    public static void GameStarted()
    {
        OnGameStarted?.Invoke();
    }
    
    public static void LevelLoaded(int levelIndex)
    {
        OnLevelLoaded?.Invoke(levelIndex);
    }
    
    // Clear all event subscriptions (useful when changing scenes)
    public static void ClearAllEvents()
    {
        OnTheseusMoved = null;
        OnTheseusWaited = null;
        OnGameWon = null;
        OnGameLost = null;
        OnLevelRestart = null;
        OnGameStarted = null;
        OnLevelLoaded = null;
    }
}