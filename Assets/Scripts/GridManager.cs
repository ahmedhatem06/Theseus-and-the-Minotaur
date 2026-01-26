using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelData currentLevel;
    
    [Header("Prefabs")] 
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private PlayerSpawner playerSpawner;

    private Theseus theseus;
    private Minotaur minotaur;

    private Cell[,] grid;
    private Vector2Int exitPos;
    
    // Grid dimensions from level data
    private int width;
    private int height;
    private float cellSize = 1f;

    private bool isAnimating;
    private bool isProcessingTurn;
    private bool gameOver;

    private void OnEnable()
    {
        GameEvents.OnTheseusMoved += TheseusMoved;
        GameEvents.OnTheseusWaited += TheseusWaited;
    }

    private void TheseusWaited()
    {
        if (isProcessingTurn) return;
        StartCoroutine(ProcessTurn());
    }

    private void OnDisable()
    {
        GameEvents.OnTheseusMoved -= TheseusMoved;
        GameEvents.OnTheseusWaited -= TheseusWaited;
    }

    private void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("Missing cell prefab");
            return;
        }

        if (playerSpawner == null)
        {
            Debug.LogError("Missing player spawner");
            return;
        }
        
        if (currentLevel == null)
        {
            Debug.LogError("No level data assigned!");
            return;
        }

        LoadLevel(currentLevel);
    }

    private void Update()
    {
        if (theseus != null && !isProcessingTurn && !gameOver)
        {
            theseus.HandleInput();
        }
    }

    private IEnumerator ProcessTurn()
    {
        isProcessingTurn = true;

        yield return StartCoroutine(minotaur.ChaseTheseus(theseus.GridPos));
        yield return StartCoroutine(minotaur.ChaseTheseus(theseus.GridPos));

        CheckGameState();

        isProcessingTurn = false;
    }

    private void TheseusMoved()
    {
        if (isProcessingTurn) return;
        StartCoroutine(ProcessTurn());
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

        // Store reference and get dimensions
        currentLevel = levelData;
        width = levelData.width;
        height = levelData.height;
        exitPos = levelData.exitPosition;

        Debug.Log($"Loading level: {levelData.levelName} (Level {levelData.levelNumber})");

        // Create and setup grid
        CreateGrid();
        LoadGridFromLevelData(levelData);
        
        // Setup characters
        SetupCharacters(levelData);
        
        // Reset game state
        gameOver = false;
        isProcessingTurn = false;
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

        if (theseus != null)
        {
            Destroy(theseus.gameObject);
            theseus = null;
        }

        if (minotaur != null)
        {
            Destroy(minotaur.gameObject);
            minotaur = null;
        }
    }

    private void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(
            (width - 1) * cellSize / 2f, 
            (height - 1) * cellSize / 2f, 
            -10);

        Camera.main.orthographicSize = Mathf.Max(width, height) * cellSize / 2f + 1f;
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

        theseus = playerSpawner.SpawnTheseus(this, theseusStartPos);
        minotaur = playerSpawner.SpawnMinotaur(this, minotaurStartPos);
        
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

    private void CheckGameState()
    {
        if (theseus.GridPos == minotaur.GridPos)
        {
            gameOver = true;
            GameEvents.GameLost();
            Debug.Log("Game Over | Lost");
        }
        else if (theseus.GridPos == exitPos)
        {
            gameOver = true;
            GameEvents.GameWon();
            Debug.Log("Game Over | Won");
        }
    }
    
    /// <summary>
    /// Public method to switch to a different level during gameplay
    /// </summary>
    public void SwitchLevel(LevelData newLevel)
    {
        LoadLevel(newLevel);
    }
}