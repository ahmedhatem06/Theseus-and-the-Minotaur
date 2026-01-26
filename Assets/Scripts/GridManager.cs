using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")] public int width = 8;
    public int height = 6;
    public float cellSize = 1f;

    [Header("Prefabs")] [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject theseusPrefab;
    [SerializeField] private GameObject minotaurPrefab;

    private GameObject theseus;
    private GameObject minotaur;

    private Cell[,] grid;
    private Vector2Int exitPos;
    private Vector2Int theseusPos;
    private Vector2Int minotaurPos;

    private void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError($"Missing cell prefab");
            return;
        }

        CreateGrid();
        SetupGrid();
        SetupCharacters();
    }

    private void SetupCharacters()
    {
        theseus = Instantiate(theseusPrefab);
        theseus.name = "theseus";

        minotaur = Instantiate(minotaurPrefab);
        minotaur.name = "minotaur";
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
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

        MoveTheseus(direction);
    }

    private void MoveTheseus(Vector2Int direction)
    {
        Vector2Int newPos = theseusPos + direction;

        if (newPos.x < 0 || newPos.x >= width || newPos.y < 0 || newPos.y >= height)
            return;

        if (!grid[newPos.x, newPos.y].CanMoveTo(theseusPos.x, theseusPos.y))
            return;

        theseusPos = newPos;

        UpdatePlayersPositions();
    }

    private void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(
            (width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, -10);
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
                    Debug.LogError($"Cell_{x}_{y} not found");
                    return;
                }

                cell.Initialize(x, y);
                grid[x, y] = cell;
            }
        }

        CenterCamera();
    }

    private void SetupGrid()
    {
        for (int x = 0; x < width; x++)
        {
            grid[x, 0].wallDown = true;
            grid[x, height - 1].wallUp = true;
        }

        for (int y = 0; y < height; y++)
        {
            grid[0, y].wallLeft = true;
            grid[width - 1, y].wallRight = true;
        }

        //walls;

        grid[2, 2].wallRight = true;
        grid[3, 2].wallLeft = true;

        grid[4, 3].wallUp = true;
        grid[4, 4].wallDown = true;

        exitPos = new Vector2Int(7, 5);
        grid[exitPos.x, exitPos.y].hasExit = true;

        //update walls;

        foreach (Cell cell in grid)
        {
            cell.UpdateVisuals();
        }
    }


    private void UpdatePlayersPositions()
    {
        theseus.transform.position = new Vector3(theseusPos.x * cellSize, theseusPos.y * cellSize, 0);
        minotaur.transform.position = new Vector3(minotaurPos.x * cellSize, minotaurPos.y * cellSize, 0);
    }
}