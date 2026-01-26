using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.3f;
    protected GridManager gridManager;
    protected Vector2Int gridPos;
    public Vector2Int GridPos => gridPos;
    protected bool isMoving = false;

    public void Initialize(GridManager grid, Vector2Int startPos)
    {
        gridManager = grid;
        gridPos = startPos;
        UpdateWorldPosition(false);
    }

    private void UpdateWorldPosition(bool animated = true)
    {
        Vector3 targetPos = gridManager.GridToWorldPos(gridPos);

        if (animated && Application.isPlaying)
        {
            StartCoroutine(AnimateToPosition(targetPos));
        }
        else
        {
            transform.position = targetPos;
        }
    }

    private IEnumerator AnimateToPosition(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            //interpolate.
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    protected bool TryMove(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        if (!gridManager.IsValidMove(gridPos, newPos))
            return false;

        gridPos = newPos;
        UpdateWorldPosition();
        return true;
    }
}