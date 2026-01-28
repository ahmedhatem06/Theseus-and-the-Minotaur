using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the history of turns for undo functionality.
/// Uses a stack to maintain chronological order of moves.
/// </summary>
public class TurnHistory : MonoBehaviour
{
    private Stack<Turn> turnStack = new();

    /// <summary>
    /// Records a completed turn to the history stack.
    /// </summary>
    /// <param name="theseusFromPos">Theseus's starting position</param>
    /// <param name="theseusToPos">Theseus's ending position</param>
    /// <param name="minotaurFromPos">Minotaur's starting position</param>
    /// <param name="minotaur1">Minotaur's position after first move</param>
    /// <param name="minotaur2">Minotaur's position after second move</param>
    /// <param name="isWait">Whether Theseus waited instead of moving</param>
    public void RecordTurn(Vector2Int theseusFromPos, Vector2Int theseusToPos, Vector2Int minotaurFromPos,
        Vector2Int minotaur1, Vector2Int minotaur2, bool isWait)
    {
        Turn turn = new Turn(theseusFromPos, theseusToPos, minotaurFromPos, minotaur1, minotaur2, isWait);
        turnStack.Push(turn);
    }

    /// <summary>
    /// Represents a single turn in the game, including both Theseus and Minotaur movements.
    /// </summary>
    public class Turn
    {
        public Vector2Int theseusFromPos;
        public Vector2Int theseusToPos;
        public Vector2Int minotaurFromPos;
        public Vector2Int minotaurAfterMove1;
        public Vector2Int minotaurAfterMove2;
        public bool wasWait;

        /// <summary>
        /// Creates a new Turn record.
        /// </summary>
        /// <param name="tFrom">Theseus start position</param>
        /// <param name="tTo">Theseus end position</param>
        /// <param name="mFrom">Minotaur start position</param>
        /// <param name="m1">Minotaur position after first move</param>
        /// <param name="m2">Minotaur position after second move</param>
        /// <param name="wait">Whether Theseus waited this turn</param>
        public Turn(Vector2Int tFrom, Vector2Int tTo, Vector2Int mFrom,
            Vector2Int m1, Vector2Int m2, bool wait)
        {
            theseusFromPos = tFrom;
            theseusToPos = tTo;
            minotaurFromPos = mFrom;
            minotaurAfterMove1 = m1;
            minotaurAfterMove2 = m2;
            wasWait = wait;
        }
    }

    /// <summary>
    /// Retrieves and removes the most recent turn from the history.
    /// Used for undo functionality.
    /// </summary>
    /// <returns>The last turn, or null if no turns are recorded</returns>
    public Turn GetLastTurn()
    {
        if (turnStack.Count > 0)
        {
            return turnStack.Pop();
        }

        return null;
    }

    /// <summary>
    /// Clears all turn history.
    /// Should be called when loading a new level or restarting.
    /// </summary>
    public void Clear()
    {
        turnStack.Clear();
    }
}