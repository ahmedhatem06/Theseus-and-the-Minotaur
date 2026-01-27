using UnityEngine;

public class Theseus : Player
{
    private Vector2Int positionBeforeMove;
    public Vector2Int GetPositionBeforeMove() => positionBeforeMove;

    public void HandleInput()
    {
        if (isMoving) return;

        //Store position before attempting to move.
        positionBeforeMove = gridPos;

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector2Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            GameEvents.TheseusWaited();
            return;
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            GameEvents.UndoRequested();
            return;
        }

        if (direction != Vector2Int.zero)
        {
            if (TryMove(direction))
            {
                GameEvents.TheseusMoved();
            }
        }
    }
}