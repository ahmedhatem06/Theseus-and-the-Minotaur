using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")] 
    public int width = 8;
    public int height = 6;
    public float cellSize = 1f;

    [Header("Prefabs")] public GameObject cellPrefab;
    private Cell[,] grid;
    private Vector2Int exitPos;

    private void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError($"Missing cell prefab");
            return;
        }

        CreateGrid();
        SetupGrid();
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
}