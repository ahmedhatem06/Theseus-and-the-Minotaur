using System.Collections;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    [SerializeField] private TurnHistory turnHistory;

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

    public void Initialize(Theseus t, Minotaur m)
    {
        theseus = t;
        minotaur = m;
    }

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

    //Animated Undo.
    private IEnumerator PerformUndo()
    {
        isProcessingTurn = true;
        TurnHistory.Turn lastTurn = turnHistory.GetLastTurn();

        if (lastTurn == null)
        {
            isProcessingTurn = false;
            yield break;
        }

        //Move theseus back.
        yield return StartCoroutine(theseus.SetPositionCoroutine(lastTurn.theseusFromPos));

        //Move Minotaur back twice.
        yield return StartCoroutine(minotaur.SetPositionCoroutine(lastTurn.minotaurAfterMove1));
        yield return StartCoroutine(minotaur.SetPositionCoroutine(lastTurn.minotaurFromPos));

        isProcessingTurn = false;
    }

    private void TheseusWaited()
    {
        if (isProcessingTurn) return;
        StartCoroutine(ProcessTurn(true));
    }

    private IEnumerator ProcessTurn(bool isWait = false)
    {
        isProcessingTurn = true;

        //Store Positions.
        Vector2Int theseusStartPos = theseus.GetPositionBeforeMove();
        Vector2Int minotaurStartPos = minotaur.GridPos;

        //Minotaur moves twice..
        yield return StartCoroutine(minotaur.ChaseTheseus(theseus.GridPos));
        Vector2Int minotaurAfterMove1 = minotaur.GridPos;

        yield return StartCoroutine(minotaur.ChaseTheseus(theseus.GridPos));
        Vector2Int minotaurAfterMove2 = minotaur.GridPos;

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