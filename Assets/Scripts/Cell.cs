using UnityEngine;

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

    public void Initialize(int x, int y)
    {
        xPos = x;
        yPos = y;

        cellRenderer = GetComponent<SpriteRenderer>();

        CreateVisuals();
    }

    private void CreateVisuals()
    {
        if (cellRenderer != null && hasExit)
        {
            cellRenderer.color = exitColor;
        }

        CreateWalls();
    }

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