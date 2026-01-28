using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Prefabs")] [SerializeField] private GameObject cellPrefab;
    private PlayerSpawner playerSpawner;
    private TurnHistory turnHistory;
    private TurnSystem turnSystem;
    private GameStateController gameStateController;
    private Camera cam;
    private Theseus theseus;
    private Minotaur minotaur;

    private Cell[,] grid;
    private Vector2Int exitPos;

    public Vector2Int ExitPos => exitPos;

    // Grid dimensions from level data
    private int width;
    private int height;
    private float cellSize = 1f;

    public static GridManager instance { get; private set; }

    public void Initialize(PlayerSpawner spawner, TurnHistory history, TurnSystem system, GameStateController gameState,
        Camera cam)
    {
        playerSpawner = spawner;
        turnHistory = history;
        turnSystem = system;
        gameStateController = gameState;
        this.cam = cam;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    /// <summary>
    /// Load a level from LevelData ScriptableObject
    /// </summary>
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("Cannot load null level data!");
            return;
        }

        // Clear existing level if any
        ClearLevel();

        // Get dimensions
        width = levelData.width;
        height = levelData.height;
        exitPos = levelData.exitPosition;

        // Create and setup grid
        CreateGrid();
        LoadGridFromLevelData(levelData);

        // Setup characters
        SetupCharacters(levelData);

        turnSystem.GetPlayers(theseus, minotaur);
        gameStateController.Initialize(this, theseus, minotaur);
        turnSystem.LevelLoaded();

        turnHistory.Clear();
    }

    /// <summary>
    /// Clear the current level
    /// </summary>
    private void ClearLevel()
    {
        if (grid != null)
        {
            foreach (Cell cell in grid)
            {
                if (cell != null)
                {
                    Destroy(cell.gameObject);
                }
            }

            grid = null;
        }
    }

    private void CenterCamera()
    {
        cam.transform.position = new Vector3(
            (width - 1) * cellSize / 2f,
            (height - 1) * cellSize / 2f,
            -10);

        cam.orthographicSize = Mathf.Max(width, height) * cellSize / 2f + 1f;
    }

    private void CreateGrid()
    {
        grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject cellObject = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cellObject.name = $"Cell_{x}_{y}";

                Cell cell = cellObject.GetComponent<Cell>();
                if (cell == null)
                {
                    Debug.LogError($"Cell_{x}_{y} does not have a Cell component!");
                    return;
                }

                cell.Initialize(x, y);
                grid[x, y] = cell;
            }
        }

        CenterCamera();
    }

    /// <summary>
    /// Load grid walls and exit from LevelData
    /// </summary>
    private void LoadGridFromLevelData(LevelData levelData)
    {
        if (levelData.cells == null || levelData.cells.Length != width * height)
        {
            Debug.LogError("Level data cells array is invalid!");
            return;
        }

        // Copy wall data from LevelData to grid cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellData cellData = levelData.GetCellData(x, y);
                if (cellData != null)
                {
                    Cell cell = grid[x, y];
                    cellData.CopyTo(cell);
                }
            }
        }

        // Update all cell visuals
        foreach (Cell cell in grid)
        {
            cell.UpdateVisuals();
        }

        Debug.Log($"Grid loaded with {width}x{height} cells");
    }

    /// <summary>
    /// Setup characters at their starting positions from LevelData
    /// </summary>
    private void SetupCharacters(LevelData levelData)
    {
        Vector2Int theseusStartPos = levelData.theseusStartPosition;
        Vector2Int minotaurStartPos = levelData.minotaurStartPosition;

        theseus = playerSpawner.SpawnTheseus(theseusStartPos);
        minotaur = playerSpawner.SpawnMinotaur(minotaurStartPos);

        Debug.Log($"Theseus spawned at {theseusStartPos}, Minotaur at {minotaurStartPos}, Exit at {exitPos}");
    }

    public Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }

    public bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        if (to.x < 0 || to.x >= width || to.y < 0 || to.y >= height)
            return false;

        // Get Direction
        Vector2Int direction = to - from;

        Cell currentCell = grid[from.x, from.y];
        if (direction == Vector2Int.up && currentCell.wallUp) return false;
        if (direction == Vector2Int.down && currentCell.wallDown) return false;
        if (direction == Vector2Int.left && currentCell.wallLeft) return false;
        if (direction == Vector2Int.right && currentCell.wallRight) return false;

        return grid[to.x, to.y].CanMoveTo(from.x, from.y);
    }
}