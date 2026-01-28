using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for player characters (Theseus and Minotaur).
/// Handles movement, animation, and position management on the grid.
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.3f;
    protected GridManager gridManager;
    protected Vector2Int gridPos;
    public Vector2Int GridPos => gridPos;
    protected bool isMoving;

    /// <summary>
    /// Initializes the player at a starting position.
    /// Gets GridManager reference and sets initial position without animation.
    /// </summary>
    /// <param name="startPos">Starting position in grid coordinates</param>
    public void Initialize(Vector2Int startPos)
    {
        gridManager = GridManager.instance;
        gridPos = startPos;
        UpdateWorldPosition(false);
    }

    /// <summary>
    /// Updates the player's world position based on its grid position.
    /// Can be animated or instantaneous.
    /// </summary>
    /// <param name="animated">Whether to animate the movement</param>
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

    /// <summary>
    /// Smoothly animates the player from its current position
    /// to the target world position.
    /// </summary>
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

    /// <summary>
    /// Attempts to move the character in a direction.
    /// Validates the move, updates grid position if valid, and animates movement.
    /// </summary>
    /// <param name="direction">Direction vector (e.g., Vector2Int.up)</param>
    /// <returns>True if move was successful, false if blocked</returns>
    protected bool TryMove(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;

        if (!gridManager.IsValidMove(gridPos, newPos))
            return false;

        gridPos = newPos;
        UpdateWorldPosition();
        return true;
    }

    /// <summary>
    /// Sets the player's grid position directly.
    /// Can optionally animate the movement.
    /// </summary>
    private void SetPosition(Vector2Int newPos, bool animated = true)
    {
        gridPos = newPos;
        UpdateWorldPosition(animated);
    }

    /// <summary>
    /// Sets the player's position and waits until movement animation finishes.
    /// </summary>
    public IEnumerator SetPositionCoroutine(Vector2Int newPos)
    {
        SetPosition(newPos);
        while (isMoving)
        {
            yield return null;
        }
    }
}