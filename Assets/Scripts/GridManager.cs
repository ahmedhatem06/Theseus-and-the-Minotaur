using UnityEngine;

/// <summary>
/// Manages the game grid, cell creation, level loading, and coordinates between game systems.
/// Acts as the central hub for spatial game logic and level data.
/// </summary>
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

    /// <summary>
    /// Gets the position of the exit cell in grid coordinates.
    /// </summary>
    public Vector2Int ExitPos => exitPos;

    // Grid dimensions from level data
    private int width;
    private int height;
    private float cellSize = 1f;

    public static GridManager instance { get; private set; }
    
    /// <summary>
    /// Initializes the GridManager with required dependencies.
    /// Should be called before loading any level.
    /// </summary>
    /// <param name="spawner">Reference to the PlayerSpawner for creating characters</param>
    /// <param name="history">Reference to TurnHistory for undo functionality</param>
    /// <param name="system">Reference to TurnSystem for game flow management</param>
    /// <param name="gameState">Reference to GameStateController for win/loss conditions</param>
    /// <param name="cam">Main camera reference for centering on the grid</param>
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
    /// Loads a level from a LevelData ScriptableObject.
    /// Clears any existing level, creates the grid, spawns characters, and initializes game systems.
    /// </summary>
    /// <param name="levelData">The ScriptableObject containing level configuration (walls, positions, etc.)</param>
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
        
        turnSystem.LevelLoaded();

        turnHistory.Clear();
    }

    /// <summary>
    /// Assigns player references to the turn system and game state controller.
    /// Must be called after characters are spawned.
    /// </summary>
    public void AssignPlayersToSystems()
    {
        turnSystem.GetPlayers(theseus, minotaur);
        gameStateController.Initialize(this, theseus, minotaur);
    }

    /// <summary>
    /// Clears the current level by destroying all cell GameObjects.
    /// Called before loading a new level.
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

    // <summary>
    /// Centers the camera on the grid and adjusts orthographic size to fit the entire level.
    /// </summary>
    private void CenterCamera()
    {
        cam.transform.position = new Vector3(
            (width - 1) * cellSize / 2f,
            (height - 1) * cellSize / 2f,
            -10);

        cam.orthographicSize = Mathf.Max(width, height) * cellSize / 2f + 1f;
    }

    /// <summary>
    /// Creates the grid of cells based on current width and height.
    /// Instantiates cell prefabs at correct positions and centers the camera.
    /// </summary>
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
    /// Loads wall and exit data from LevelData into the grid cells.
    /// Copies CellData to each Cell component and updates visual representation.
    /// </summary>
    /// <param name="levelData">The level data containing wall configurations</param>
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
    /// Spawns Theseus and the Minotaur at their starting positions defined in the level data.
    /// </summary>
    /// <param name="levelData">Level data containing character starting positions</param>
    private void SetupCharacters(LevelData levelData)
    {
        Vector2Int theseusStartPos = levelData.theseusStartPosition;
        Vector2Int minotaurStartPos = levelData.minotaurStartPosition;

        theseus = playerSpawner.SpawnTheseus(theseusStartPos);
        minotaur = playerSpawner.SpawnMinotaur(minotaurStartPos);

        Debug.Log($"Theseus spawned at {theseusStartPos}, Minotaur at {minotaurStartPos}, Exit at {exitPos}");
    }

    /// <summary>
    /// Converts grid coordinates to world position.
    /// </summary>
    /// <param name="gridPos">Position in grid coordinates (x, y)</param>
    /// <returns>World position as Vector3</returns>
    public Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
    }

    
    /// <summary>
    /// Checks if a move from one grid position to another is valid.
    /// Validates bounds, wall blocking, and entry permissions.
    /// </summary>
    /// <param name="from">Starting grid position</param>
    /// <param name="to">Target grid position</param>
    /// <returns>True if the move is valid, false otherwise</returns>
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