using System.Collections.Generic;
using UnityEngine;

public class TurnHistory : MonoBehaviour
{
    private Stack<Turn> turnStack = new();

    public void RecordTurn(Vector2Int theseusFromPos, Vector2Int theseusToPos, Vector2Int minotaurFromPos,
        Vector2Int minotaur1, Vector2Int minotaur2, bool isWait)
    {
        Turn turn = new Turn(theseusFromPos, theseusToPos, minotaurFromPos, minotaur1, minotaur2, isWait);
        turnStack.Push(turn);
    }

    public class Turn
    {
        public Vector2Int theseusFromPos;
        public Vector2Int theseusToPos;
        public Vector2Int minotaurFromPos;
        public Vector2Int minotaurAfterMove1;
        public Vector2Int minotaurAfterMove2;
        public bool wasWait;

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

    public Turn GetLastTurn()
    {
        if (turnStack.Count > 0)
        {
            return turnStack.Pop();
        }

        return null;
    }

    public void Clear()
    {
        turnStack.Clear();
    }
}