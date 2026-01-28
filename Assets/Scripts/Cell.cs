using UnityEngine;

/// <summary>
/// Represents a single cell in the game grid.
/// Manages walls, exit status, and visual representation of the cell.
/// </summary>
public class Cell : MonoBehaviour
{
    public bool wallUp;
    public bool wallDown;
    public bool wallLeft;
    public bool wallRight;

    public bool hasExit;

    public int xPos;
    public int yPos;

    public Color wallColor = Color.black;
    public Color exitColor = Color.green;
    public float wallThickness = 0.1f;

    private SpriteRenderer cellRenderer;
    private GameObject wallUpObject;
    private GameObject wallDownObject;
    private GameObject wallLeftObject;
    private GameObject wallRightObject;

    /// <summary>
    /// Initializes the cell with its grid position and creates visual representation.
    /// </summary>
    /// <param name="x">X coordinate in the grid (0-based)</param>
    /// <param name="y">Y coordinate in the grid (0-based)</param>
    public void Initialize(int x, int y)
    {
        xPos = x;
        yPos = y;

        cellRenderer = GetComponent<SpriteRenderer>();

        CreateVisuals();
    }

    /// <summary>
    /// Creates the initial visual representation of the cell.
    /// Sets exit color if applicable and creates wall GameObjects.
    /// </summary>
    private void CreateVisuals()
    {
        if (cellRenderer != null && hasExit)
        {
            cellRenderer.color = exitColor;
        }

        CreateWalls();
    }

    /// <summary>
    /// Creates wall GameObjects for all walls that are enabled.
    /// Each wall is a child GameObject with a SpriteRenderer.
    /// </summary>
    private void CreateWalls()
    {
        if (wallUp)
        {
            CreateWall(ref wallUpObject, "WallUp", new Vector3(0, 0.5f, 0),
                new Vector3(1 + wallThickness, wallThickness, 1));
        }

        if (wallDown)
        {
            CreateWall(ref wallDownObject, "WallDown", new Vector3(0, -0.5f, 0),
                new Vector3(1 + wallThickness, wallThickness, 1));
        }

        if (wallRight)
        {
            CreateWall(ref wallRightObject, "WallRight", new Vector3(0.5f, 0, 0),
                new Vector3(wallThickness, 1 + wallThickness, 1));
        }

        if (wallLeft)
        {
            CreateWall(ref wallLeftObject, "WallLeft", new Vector3(-0.5f, 0, 0),
                new Vector3(wallThickness, 1 + wallThickness, 1));
        }
    }

    /// <summary>
    /// Creates a single wall GameObject with specified position and scale.
    /// </summary>
    /// <param name="wall">Reference to store the created wall GameObject</param>
    /// <param name="name">Name for the wall GameObject</param>
    /// <param name="localPosition">Local position relative to cell center</param>
    /// <param name="scale">Scale of the wall (length, thickness, depth)</param>
    private void CreateWall(ref GameObject wall, string name, Vector3 localPosition, Vector3 scale)
    {
        wall = new GameObject(name);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = localPosition;
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = scale;

        SpriteRenderer wallRenderer = wall.AddComponent<SpriteRenderer>();
        wallRenderer.sprite = CreateSquareSprite();
        wallRenderer.color = wallColor;
        wallRenderer.sortingOrder = 1;
    }

    /// <summary>
    /// Creates a simple 1x1 white square sprite for walls.
    /// </summary>
    /// <returns>A sprite that can be colored and scaled</returns>
    private Sprite CreateSquareSprite()
    {
        // Create texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        // Create sprite
        return Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f), 1f
        );
    }

    /// <summary>
    /// Updates the visual representation of the cell.
    /// Should be called when wall or exit status changes.
    /// Updates exit color and adds/removes wall GameObjects as needed.
    /// </summary>
    public void UpdateVisuals()
    {
        if (cellRenderer != null && hasExit)
        {
            cellRenderer.color = exitColor;
        }
        else if (cellRenderer != null && !hasExit)
        {
            cellRenderer.color = Color.white;
        }
        
        UpdateWall(wallUp, ref wallUpObject, "WallUp", new Vector3(0, 0.5f, 0),
            new Vector3(1 + wallThickness, wallThickness, 1));


        UpdateWall(wallDown, ref wallDownObject, "WallDown", new Vector3(0, -0.5f, 0),
            new Vector3(1 + wallThickness, wallThickness, 1));


        UpdateWall(wallRight, ref wallRightObject, "WallRight", new Vector3(0.5f, 0, 0),
            new Vector3(wallThickness, 1 + wallThickness, 1));


        UpdateWall(wallLeft, ref wallLeftObject, "WallLeft", new Vector3(-0.5f, 0, 0),
            new Vector3(wallThickness, 1 + wallThickness, 1));
    }

    /// <summary>
    /// Updates a single wall - creates it if needed, destroys it if not needed.
    /// </summary>
    /// <param name="shouldExist">Whether the wall should exist</param>
    /// <param name="wall">Reference to the wall GameObject</param>
    /// <param name="name">Name for the wall if it needs to be created</param>
    /// <param name="localPos">Local position for the wall</param>
    /// <param name="scale">Scale for the wall</param>
    private void UpdateWall(bool isExit, ref GameObject wall, string name, Vector3 localPos, Vector3 scale)
    {
        if (isExit && wall == null)
        {
            CreateWall(ref wall, name, localPos, scale);
        }
        else if (!isExit && wall != null)
        {
            Destroy(wall);
            wall = null;
        }
    }

    /// <summary>
    /// Checks if a character can enter this cell from a specific position.
    /// Validates that the entry direction doesn't have a blocking wall.
    /// </summary>
    /// <param name="fromX">X coordinate of the position trying to enter</param>
    /// <param name="fromY">Y coordinate of the position trying to enter</param>
    /// <returns>True if entry is allowed, false if blocked by a wall</returns>
    public bool CanMoveTo(int fromX, int fromY)
    {
        //Check which direction we're coming from and make sure there are no walls blocking it.

        if (fromX < xPos && wallLeft) return false;
        if (fromX > xPos && wallRight) return false;
        if (fromY < yPos && wallDown) return false;
        if (fromY > yPos && wallUp) return false;
        return true;
    }
}