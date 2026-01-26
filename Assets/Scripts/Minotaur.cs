using System.Collections;
using UnityEngine;

public class Minotaur : Player
{
    private Vector2Int theseusPos;

    public IEnumerator ChaseTheseus(Vector2Int targetPos)
    {
        theseusPos = targetPos;
        Vector2Int direction;

        int horizontalMovement = theseusPos.x - gridPos.x;
        int verticalMovement = theseusPos.y - gridPos.y;

        //Horizontal movement first.
        if (horizontalMovement != 0)
        {
            direction = horizontalMovement > 0 ? Vector2Int.right : Vector2Int.left;
            if (TryMove(direction))
            {
                while (isMoving)
                {
                    yield return null;
                }

                yield break;
            }
        }

        //Try vertical if horizontal failed.
        if (verticalMovement != 0)
        {
            direction = verticalMovement > 0 ? Vector2Int.up : Vector2Int.down;
            if (TryMove(direction))
            {
                while (isMoving)
                {
                    yield return null;
                }
            }
        }
    }
}