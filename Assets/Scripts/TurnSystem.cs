using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the turn-based game flow.
/// Handles player movement, Minotaur AI response, turn processing, and undo functionality.
/// </summary>
public class TurnSystem : MonoBehaviour
{
    private TurnHistory turnHistory;

    private Theseus theseus;
    private Minotaur minotaur;

    private bool isProcessingTurn;
    private bool gameOver;

    private void OnEnable()
    {
        GameEvents.OnTheseusMoved += TheseusMoved;
        GameEvents.OnTheseusWaited += TheseusWaited;
        GameEvents.OnUndoRequested += UndoRequested;
        GameEvents.OnGameLost += GameOver;
        GameEvents.OnGameWon += GameOver;
    }

    private void GameOver()
    {
        gameOver = true;
    }

    private void OnDisable()
    {
        GameEvents.OnTheseusMoved -= TheseusMoved;
        GameEvents.OnTheseusWaited -= TheseusWaited;
        GameEvents.OnUndoRequested -= UndoRequested;
        GameEvents.OnGameLost -= GameOver;
        GameEvents.OnGameWon -= GameOver;
    }

    public void Initialize(TurnHistory history)
    {
        turnHistory = history;
    }

    public void GetPlayers(Theseus t, Minotaur m)
    {
        theseus = t;
        minotaur = m;
    }
    
    /// <summary>
    /// Polls for player input each frame.
    /// Only accepts input when not processing a turn and game is not over.
    /// </summary>
    private void Update()
    {
        if (theseus != null && !isProcessingTurn && !gameOver)
        {
            theseus.HandleInput();
        }
    }
    
    private void UndoRequested()
    {
        StartCoroutine(PerformUndo());
    }
    
    /// <summary>
    // /// Performs an animated undo of the last turn.
    // /// Moves Theseus back first, then Minotaur moves back twice in reverse order.
    // /// </summary>
    // /// <returns>Coroutine that completes when undo animation is finished</returns>
    private IEnumerator PerformUndo()
    {
        isProcessingTurn = true;
        TurnHistory.Turn lastTurn = turnHistory.GetLastTurn();

        if (lastTurn == null)
        {
            isProcessingTurn = false;
            yield break;
        }

        // Move Theseus back to previous position
        yield return StartCoroutine(theseus.SetPositionCoroutine(lastTurn.theseusFromPos));

        // Move Minotaur back - reverse order (second move first, then first move)
        yield return StartCoroutine(minotaur.SetPositionCoroutine(lastTurn.minotaurAfterMove1));
        yield return StartCoroutine(minotaur.SetPositionCoroutine(lastTurn.minotaurFromPos));

        isProcessingTurn = false;
    }

    /// <summary>
    /// Event handler called when Theseus waits (W key).
    /// Processes turn without Theseus moving.
    /// </summary>
    private void TheseusWaited()
    {
        if (isProcessingTurn) return;
        StartCoroutine(ProcessTurn(true));
    }

    /// <summary>
    /// Processes a complete turn: records positions, moves Minotaur twice, records history.
    /// </summary>
    /// <param name="isWait">True if Theseus waited instead of moving</param>
    /// <returns>Coroutine that completes when turn processing is finished</returns>
    private IEnumerator ProcessTurn(bool isWait = false)
    {
        isProcessingTurn = true;

        // Store positions before Minotaur moves
        Vector2Int theseusStartPos = theseus.GetPositionBeforeMove();
        Vector2Int minotaurStartPos = minotaur.GridPos;

        // Minotaur makes first move toward Theseus
        yield return StartCoroutine(minotaur.ChaseTheseus(theseus.GridPos));
        Vector2Int minotaurAfterMove1 = minotaur.GridPos;

        // Minotaur makes second move toward Theseus
        yield return StartCoroutine(minotaur.ChaseTheseus(theseus.GridPos));
        Vector2Int minotaurAfterMove2 = minotaur.GridPos;

        // Record this turn for undo functionality
        turnHistory.RecordTurn(
            theseusStartPos,
            theseus.GridPos,
            minotaurStartPos,
            minotaurAfterMove1,
            minotaurAfterMove2,
            isWait);

        GameEvents.TurnCompleted();

        isProcessingTurn = false;
    }
    
    /// <summary>
    /// Event handler called when Theseus successfully moves.
    /// Initiates turn processing if not already in progress.
    /// </summary>
    private void TheseusMoved()
    {
        if (isProcessingTurn) return;
        StartCoroutine(ProcessTurn());
    }

    public void LevelLoaded()
    {
        gameOver = false;
    }
}