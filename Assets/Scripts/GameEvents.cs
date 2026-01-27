using System;

public static class GameEvents
{
    public static event Action OnTheseusMoved;
    public static event Action OnTheseusWaited;
    public static event Action OnGameWon;
    public static event Action OnGameLost;
    public static event Action OnGameStarted;
    public static event Action<int> OnLevelLoaded;

    public static void TheseusMoved()
    {
        OnTheseusMoved?.Invoke();
    }

    public static void GameWon()
    {
        OnGameWon?.Invoke();
    }

    public static void GameLost()
    {
        OnGameLost?.Invoke();
    }

    public static void TheseusWaited()
    {
        OnTheseusWaited?.Invoke();
    }

    public static void LevelLoaded(int level)
    {
        OnLevelLoaded?.Invoke(level);
    }
    
    public static void GameStarted()
    {
        OnGameStarted?.Invoke();
    }
}