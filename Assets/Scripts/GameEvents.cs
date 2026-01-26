using System;

public static class GameEvents
{
    public static event Action OnTheseusMoved;
    public static event Action OnGameWon;
    public static event Action OnGameLost;

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
}