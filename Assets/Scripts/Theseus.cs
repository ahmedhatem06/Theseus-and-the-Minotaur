using UnityEngine;

public class Theseus : Player
{
    public void HandleInput()
    {
        if (isMoving) return;
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
            //Wait.
        }

        if (direction != Vector2Int.zero)
        {
            if (TryMove(direction))
            {
                GameEvents.TheseusMoved();
            }
        }
    }

    private void Lost()
    {
        GameEvents.GameLost();
    }

    private void Won()
    {
        GameEvents.GameWon();
    }
}