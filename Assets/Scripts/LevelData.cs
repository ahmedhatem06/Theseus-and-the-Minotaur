using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Theseus and Minotaur/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public int levelNumber = 1;
    public string levelDescription;
    
    [Header("Grid Size")]
    public int width = 8;
    public int height = 6;
    
    [Header("Starting Positions")]
    [SerializeField] private Vector2Int _theseusStartPosition = new Vector2Int(1, 1);
    [SerializeField] private Vector2Int _minotaurStartPosition = new Vector2Int(6, 4);
    [SerializeField] private Vector2Int _exitPosition = new Vector2Int(7, 5);
    
    public Vector2Int theseusStartPosition
    {
        get => _theseusStartPosition;
        set
        {
            _theseusStartPosition = new Vector2Int(
                Mathf.Clamp(value.x, 0, width - 1),
                Mathf.Clamp(value.y, 0, height - 1)
            );
        }
    }
    
    public Vector2Int minotaurStartPosition
    {
        get => _minotaurStartPosition;
        set
        {
            _minotaurStartPosition = new Vector2Int(
                Mathf.Clamp(value.x, 0, width - 1),
                Mathf.Clamp(value.y, 0, height - 1)
            );
        }
    }
    
    public Vector2Int exitPosition
    {
        get => _exitPosition;
        set
        {
            _exitPosition = new Vector2Int(
                Mathf.Clamp(value.x, 0, width - 1),
                Mathf.Clamp(value.y, 0, height - 1)
            );
            UpdateExitCell();
        }
    }
    
    [Header("Grid Layout")]
    [Tooltip("Grid is stored row by row, from bottom to top (y=0 at bottom)")]
    public CellData[] cells;
    
    public void OnValidate()
    {
        // Clamp width and height to positive values
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        
        // Ensure the cells array matches the grid size
        int requiredSize = width * height;
        if (cells == null || cells.Length != requiredSize)
        {
            System.Array.Resize(ref cells, requiredSize);
            
            // Initialize new cells
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                {
                    cells[i] = new CellData();
                }
            }
        }
        
        // Clamp all positions using the properties (which will trigger UpdateExitCell)
        theseusStartPosition = _theseusStartPosition;
        minotaurStartPosition = _minotaurStartPosition;
        exitPosition = _exitPosition;
    }
    
    private void UpdateExitCell()
    {
        if (cells == null || cells.Length != width * height)
            return;
        
        // Clear all exits first
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] != null)
            {
                cells[i].hasExit = false;
            }
        }
        
        // Set exit at the exit position
        CellData exitCell = GetCellData(exitPosition.x, exitPosition.y);
        if (exitCell != null)
        {
            exitCell.hasExit = true;
        }
    }
    
    public CellData GetCellData(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;
        
        int index = y * width + x;
        return cells[index];
    }
    
    public void SetCellData(int x, int y, CellData data)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
        
        int index = y * width + x;
        cells[index] = data;
    }
    
    [ContextMenu("Create Border Walls")]
    public void CreateBorderWalls()
    {
        OnValidate(); // Ensure array is correct size
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellData cell = GetCellData(x, y);
                if (cell == null) continue;
                
                // Bottom border - cells at y=0 have a wall on their bottom
                if (y == 0)
                    cell.wallDown = true;
                
                // Top border - cells at y=height-1 have a wall on their top
                if (y == height - 1)
                    cell.wallUp = true;
                
                // Left border - cells at x=0 have a wall on their left
                if (x == 0)
                    cell.wallLeft = true;
                
                // Right border - cells at x=width-1 have a wall on their right
                if (x == width - 1)
                    cell.wallRight = true;
            }
        }
        
        Debug.Log("Border walls created!");
    }
    
    [ContextMenu("Clear All Walls")]
    public void ClearAllWalls()
    {
        OnValidate();
        
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] != null)
            {
                cells[i].wallUp = false;
                cells[i].wallDown = false;
                cells[i].wallLeft = false;
                cells[i].wallRight = false;
                cells[i].hasExit = false;
            }
        }
        
        Debug.Log("All walls cleared!");
    }
    
    [ContextMenu("Set Exit at Exit Position")]
    public void SetExitAtPosition()
    {
        OnValidate();
        
        // Clear all exits first
        foreach (CellData cell in cells)
        {
            if (cell != null)
                cell.hasExit = false;
        }
        
        // Set the exit at the specified position
        CellData exitCell = GetCellData(exitPosition.x, exitPosition.y);
        if (exitCell != null)
        {
            exitCell.hasExit = true;
            Debug.Log($"Exit set at ({exitPosition.x}, {exitPosition.y})");
        }
        else
        {
            Debug.LogWarning($"Invalid exit position ({exitPosition.x}, {exitPosition.y})");
        }
    }
}