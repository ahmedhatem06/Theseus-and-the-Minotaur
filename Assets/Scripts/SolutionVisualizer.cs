using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolutionVisualizer : MonoBehaviour
{
    [Header("Path Colors")]
    [SerializeField] private Color theseusPathColor = new Color(0f, 1f, 0f, 0.7f);
    [SerializeField] private Color minotaurPathColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float pathWidth = 0.15f;
    
    [Header("Position Markers")]
    [SerializeField] private Color theseusMarkerColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color minotaurMarkerColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float markerSize = 0.3f;
    
    [Header("Animation")]
    [SerializeField] private float stepDelay = 0.8f;
    
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private MazeSolver solver;
    
    private LineRenderer theseusPathLine;
    private LineRenderer minotaurPathLine;
    private List<GameObject> positionMarkers = new List<GameObject>();
    private List<MazeSolver.Move> currentSolution;
    private bool isPlayingSolution = false;
    
    void Start()
    {
        SetupLineRenderers();
        
        if (solver == null)
        {
            solver = gameObject.AddComponent<MazeSolver>();
        }
    }
    
    void Update()
    {
        // Press S to solve and show solution
        if (Input.GetKeyDown(KeyCode.S))
        {
            SolveAndVisualize();
        }
        
        // Press H to hide solution
        if (Input.GetKeyDown(KeyCode.H))
        {
            HideSolution();
        }
        
        // Press Space to play solution step by step
        if (Input.GetKeyDown(KeyCode.Space) && currentSolution != null && !isPlayingSolution)
        {
            StartCoroutine(PlaySolutionAnimation());
        }
    }
    
    private void SetupLineRenderers()
    {
        // Create Theseus path line
        GameObject theseusPathObj = new GameObject("TheseusPath");
        theseusPathObj.transform.SetParent(transform);
        theseusPathLine = theseusPathObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(theseusPathLine, theseusPathColor);
        
        // Create Minotaur path line
        GameObject minotaurPathObj = new GameObject("MinotaurPath");
        minotaurPathObj.transform.SetParent(transform);
        minotaurPathLine = minotaurPathObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(minotaurPathLine, minotaurPathColor);
    }
    
    private void ConfigureLineRenderer(LineRenderer lr, Color color)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = pathWidth;
        lr.endWidth = pathWidth;
        lr.sortingOrder = 1;
        lr.useWorldSpace = true;
        lr.enabled = false;
    }
    
    public void SolveAndVisualize()
    {
        LevelData currentLevel = GetCurrentLevel();
        
        if (currentLevel == null)
        {
            Debug.LogError("No level loaded!");
            return;
        }
        
        Debug.Log($"Solving level: {currentLevel.levelName}");
        HideSolution();
        
        List<MazeSolver.Move> solution = solver.SolveMaze(currentLevel);
        
        if (solution != null && solution.Count > 0)
        {
            currentSolution = solution;
            DrawCompleteSolution(solution, currentLevel);
        }
        else
        {
            Debug.LogWarning("No solution found for this level!");
        }
    }
    
    private void DrawCompleteSolution(List<MazeSolver.Move> moves, LevelData level)
    {
        // Build Theseus path
        List<Vector3> theseusPositions = new List<Vector3>();
        theseusPositions.Add(gridManager.GridToWorldPos(level.theseusStartPosition));
        
        // Build Minotaur path (tracking all positions including intermediate moves)
        List<Vector3> minotaurPositions = new List<Vector3>();
        minotaurPositions.Add(gridManager.GridToWorldPos(level.minotaurStartPosition));
        
        foreach (var move in moves)
        {
            // Add Theseus position
            theseusPositions.Add(gridManager.GridToWorldPos(move.theseusTo));
            
            // Add Minotaur's two move positions
            minotaurPositions.Add(gridManager.GridToWorldPos(move.minotaurAfterMove1));
            minotaurPositions.Add(gridManager.GridToWorldPos(move.minotaurAfterMove2));
        }
        
        // Draw Theseus path
        theseusPathLine.enabled = true;
        theseusPathLine.positionCount = theseusPositions.Count;
        theseusPathLine.SetPositions(theseusPositions.ToArray());
        
        // Draw Minotaur path
        minotaurPathLine.enabled = true;
        minotaurPathLine.positionCount = minotaurPositions.Count;
        minotaurPathLine.SetPositions(minotaurPositions.ToArray());
        
        Debug.Log($"Solution drawn: Theseus {theseusPositions.Count} positions, Minotaur {minotaurPositions.Count} positions");
    }
    
    public void HideSolution()
    {
        theseusPathLine.enabled = false;
        minotaurPathLine.enabled = false;
        ClearMarkers();
        currentSolution = null;
    }
    
    private IEnumerator PlaySolutionAnimation()
    {
        if (currentSolution == null || currentSolution.Count == 0)
            yield break;
        
        isPlayingSolution = true;
        HideSolution();
        ClearMarkers();
        
        LevelData level = GetCurrentLevel();
        
        Debug.Log("=== PLAYING SOLUTION ANIMATION ===");
        
        // Show initial positions
        CreateMarker(level.theseusStartPosition, theseusMarkerColor, "Theseus Start");
        CreateMarker(level.minotaurStartPosition, minotaurMarkerColor, "Minotaur Start");
        
        yield return new WaitForSeconds(stepDelay);
        
        // Animate each move
        for (int i = 0; i < currentSolution.Count; i++)
        {
            var move = currentSolution[i];
            
            ClearMarkers();
            
            // Show move info
            string action = move.isWait ? "WAIT" : $"Move {GetDirectionName(move.theseusFrom, move.theseusTo)}";
            Debug.Log($"Step {i + 1}/{currentSolution.Count}: {action}");
            
            // Show current positions
            CreateMarker(move.theseusTo, theseusMarkerColor, $"T-{i + 1}");
            CreateMarker(move.minotaurAfterMove2, minotaurMarkerColor, $"M-{i + 1}");
            
            // Draw paths up to this point
            DrawPartialSolution(i + 1);
            
            yield return new WaitForSeconds(stepDelay);
        }
        
        Debug.Log("Animation complete!");
        isPlayingSolution = false;
    }
    
    private void DrawPartialSolution(int steps)
    {
        if (currentSolution == null || steps == 0) return;
        
        LevelData level = GetCurrentLevel();
        
        // Build partial paths
        List<Vector3> theseusPath = new List<Vector3>();
        theseusPath.Add(gridManager.GridToWorldPos(level.theseusStartPosition));
        
        List<Vector3> minotaurPath = new List<Vector3>();
        minotaurPath.Add(gridManager.GridToWorldPos(level.minotaurStartPosition));
        
        for (int i = 0; i < Mathf.Min(steps, currentSolution.Count); i++)
        {
            var move = currentSolution[i];
            theseusPath.Add(gridManager.GridToWorldPos(move.theseusTo));
            minotaurPath.Add(gridManager.GridToWorldPos(move.minotaurAfterMove1));
            minotaurPath.Add(gridManager.GridToWorldPos(move.minotaurAfterMove2));
        }
        
        theseusPathLine.enabled = true;
        theseusPathLine.positionCount = theseusPath.Count;
        theseusPathLine.SetPositions(theseusPath.ToArray());
        
        minotaurPathLine.enabled = true;
        minotaurPathLine.positionCount = minotaurPath.Count;
        minotaurPathLine.SetPositions(minotaurPath.ToArray());
    }
    
    private void CreateMarker(Vector2Int gridPos, Color color, string label)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = label;
        marker.transform.position = gridManager.GridToWorldPos(gridPos);
        marker.transform.localScale = Vector3.one * markerSize;
        marker.transform.SetParent(transform);
        
        Renderer renderer = marker.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = color;
        renderer.sortingOrder = 2;
        
        Destroy(marker.GetComponent<Collider>());
        
        positionMarkers.Add(marker);
    }
    
    private void ClearMarkers()
    {
        foreach (var marker in positionMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        positionMarkers.Clear();
    }
    
    private string GetDirectionName(Vector2Int from, Vector2Int to)
    {
        Vector2Int dir = to - from;
        if (dir == Vector2Int.up) return "UP";
        if (dir == Vector2Int.down) return "DOWN";
        if (dir == Vector2Int.left) return "LEFT";
        if (dir == Vector2Int.right) return "RIGHT";
        return "NONE";
    }
    
    private LevelData GetCurrentLevel()
    {
        LevelsManager levelsManager = FindObjectOfType<LevelsManager>();
        return levelsManager?.GetCurrentLevelData();
    }
}