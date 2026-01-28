using System;
using System.Collections;
using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    [Header("Level Data")] [SerializeField]
    private LevelData[] levels;

    [Header("Level Transition")] [SerializeField]
    private float levelTransitionDelay = 1.5f;

    [SerializeField] private float restartDelay = 1.5f;
    
    private GridManager gridManager;

    private int currentLevelIndex;

    private void OnEnable()
    {
        GameEvents.OnGameWon += OnGameWon;
        GameEvents.OnGameLost += OnGameLost;
        GameEvents.OnGameStarted += OnGameStarted;
    }
    
    public LevelData GetCurrentLevelData()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex];
        }
        return null;
    }

    private void OnGameStarted()
    {
        gridManager = GridManager.instance;
        
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("No levels assigned!");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("No grid manager assigned!");
            return;
        }

        LoadCurrentLevel();

        gridManager.AssignPlayersToSystems();
    }

    private void OnDisable()
    {
        GameEvents.OnGameWon -= OnGameWon;
        GameEvents.OnGameLost -= OnGameLost;
        GameEvents.OnGameStarted -= OnGameStarted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Restarting level " + currentLevelIndex);
            RestartCurrentLevel();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Loading next level " + currentLevelIndex);
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Loading previous level " + currentLevelIndex);
            LoadPreviousLevel();
        }
    }

    private void LoadCurrentLevel()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Length)
        {
            Debug.LogError($"Invalid level index: {currentLevelIndex}");
            return;
        }

        LevelData levelData = levels[currentLevelIndex];

        if (levelData == null)
        {
            Debug.LogError($"Level at index {currentLevelIndex} is null!");
            return;
        }

        Debug.Log($"Loading level {levelData.name}");

        gridManager.LoadLevel(levelData);

        GameEvents.LevelLoaded(currentLevelIndex);
    }

    /// <summary>
    /// Restart the current level after a delay
    /// </summary>
    private IEnumerator RestartLevelWithDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        LoadCurrentLevel();
    }

    /// <summary>
    /// Load the next level after a delay
    /// </summary>
    private IEnumerator LoadNextLevelWithDelay()
    {
        yield return new WaitForSeconds(levelTransitionDelay);
        LoadCurrentLevel();
    }

    /// <summary>
    /// Called when the player wins the current level
    /// </summary>
    private void OnGameWon()
    {
        Debug.Log($"Level {currentLevelIndex + 1} completed!");

        // Move to next level
        currentLevelIndex++;

        // Check if there are more levels
        if (currentLevelIndex < levels.Length)
        {
            Debug.Log($"Loading next level: {currentLevelIndex + 1}/{levels.Length}");
            StartCoroutine(LoadNextLevelWithDelay());
        }
        else
        {
            Debug.Log("All levels completed! Congratulations!");
        }
    }

    /// <summary>
    /// Called when the player loses (caught by Minotaur)
    /// </summary>
    private void OnGameLost()
    {
        Debug.Log($"Game Over! Restarting level {currentLevelIndex + 1}");
        StartCoroutine(RestartLevelWithDelay());
    }

    private void LoadPreviousLevel()
    {
        if (currentLevelIndex > 0)
        {
            currentLevelIndex--;
            LoadCurrentLevel();
        }
        else
        {
            Debug.Log("Already at the first level!");
        }
    }

    private void LoadNextLevel()
    {
        if (currentLevelIndex < levels.Length - 1)
        {
            currentLevelIndex++;
            LoadCurrentLevel();
        }
        else
        {
            Debug.Log("Already at the last level!");
        }
    }

    private void RestartCurrentLevel()
    {
        LoadCurrentLevel();
    }
}