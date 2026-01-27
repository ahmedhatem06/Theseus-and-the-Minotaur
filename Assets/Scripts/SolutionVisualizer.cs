using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolutionVisualizer : MonoBehaviour
{
    [Header("Path Colors")] [SerializeField]
    private Color theseusPathColor = new Color(0f, 1f, 0f, 0.7f);

    [SerializeField] private Color minotaurPathColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float pathWidth = 0.15f;

    [Header("Position Markers")] [SerializeField]
    private Color theseusMarkerColor = new Color(0f, 1f, 0f, 0.3f);

    [SerializeField] private Color minotaurMarkerColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color waitMarkerColor = new Color(1f, 1f, 0f, 0.8f); // Yellow for wait
    [SerializeField] private float markerSize = 0.3f;
    [SerializeField] private float waitMarkerSize = 0.4f;

    [Header("Animation")] [SerializeField] private float stepDelay = 0.8f;

    [Header("References")] [SerializeField]
    private GridManager gridManager;

    [SerializeField] private LevelsManager levelsManager;

    [SerializeField] private MazeSolver solver;

    private LineRenderer theseusPathLine;
    private LineRenderer minotaurPathLine;
    private List<GameObject> positionMarkers = new();
    private List<GameObject> waitMarkers = new(); // Separate list for wait markers
    private List<MazeSolver.Move> currentSolution;
    private bool isPlayingSolution;
    private Coroutine playRoutine;
    void Start()
    {
        SetupLineRenderers();

        if (solver == null)
        {
            solver = gameObject.AddComponent<MazeSolver>();
        }

        // Find GridManager if not assigned
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found! Please assign it in the inspector.");
            }
        }

        // Subscribe to level events to clear solution when level changes
        GameEvents.OnLevelLoaded += OnLevelChanged;
        GameEvents.OnGameWon += OnLevelChanged;
        GameEvents.OnGameLost += OnLevelChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnLevelLoaded -= OnLevelChanged;
        GameEvents.OnGameWon -= OnLevelChanged;
        GameEvents.OnGameLost -= OnLevelChanged;
    }

    private void OnLevelChanged(int levelIndex)
    {
        StopAndClearSolution();
    }
    
    private void StopAndClearSolution()
    {
        // Stop animation coroutine if running
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        // Extra safety: stop any stray coroutines
        StopAllCoroutines();

        isPlayingSolution = false;
        currentSolution = null;

        HideSolution();
    }

    private void OnLevelChanged()
    {
        StopAndClearSolution();
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
        if (Input.GetKeyDown(KeyCode.T) && currentSolution != null && !isPlayingSolution)
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

    private void SolveAndVisualize()
    {
        LevelData currentLevel = GetCurrentLevel();

        if (currentLevel == null)
        {
            Debug.LogError("No level loaded!");
            return;
        }

        // Stop any running animation
        if (isPlayingSolution)
        {
            StopAllCoroutines();
            isPlayingSolution = false;
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
        if (gridManager == null)
        {
            Debug.LogError("GridManager is null! Cannot draw solution.");
            return;
        }

        // Clear any existing markers
        ClearMarkers();

        // Build Theseus path - add duplicate positions for wait moves
        List<Vector3> theseusPositions = new List<Vector3>
        {
            gridManager.GridToWorldPos(level.theseusStartPosition)
        };

        // Build Minotaur path (tracking all positions including intermediate moves)
        List<Vector3> minotaurPositions = new List<Vector3>
        {
            gridManager.GridToWorldPos(level.minotaurStartPosition)
        };

        foreach (var move in moves)
        {
            // Add Theseus position (stays same for wait)
            theseusPositions.Add(gridManager.GridToWorldPos(move.theseusTo));

            // Mark wait positions with yellow marker
            if (move.isWait)
            {
                CreateWaitMarker(move.theseusTo, "WAIT");
            }

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

        Debug.Log(
            $"Solution drawn: Theseus {theseusPositions.Count} positions, Minotaur {minotaurPositions.Count} positions");
    }

    private void HideSolution()
    {
        if (theseusPathLine != null)
            theseusPathLine.enabled = false;

        if (minotaurPathLine != null)
            minotaurPathLine.enabled = false;

        ClearMarkers();
        isPlayingSolution = false;
    }

    private IEnumerator PlaySolutionAnimation()
    {
        if (isPlayingSolution)
            yield break;

        isPlayingSolution = true;
        
        if (currentSolution == null || currentSolution.Count == 0)
        {
            Debug.LogWarning("No solution to animate! Press S to solve first.");
            yield break;
        }

        LevelData level = GetCurrentLevel();
        if (level == null)
        {
            Debug.LogError("Cannot animate - no level loaded!");
            yield break;
        }

        // Stop any existing animation and clear everything
        isPlayingSolution = true;
        theseusPathLine.enabled = false;
        minotaurPathLine.enabled = false;
        ClearMarkers();
        ClearWaitMarkers();

        Debug.Log("=== PLAYING SOLUTION ANIMATION ===");

        // Pre-create all wait markers so they stay visible throughout animation
        for (int i = 0; i < currentSolution.Count; i++)
        {
            if (currentSolution[i].isWait)
            {
                CreateWaitMarker(currentSolution[i].theseusTo, $"WAIT-{i + 1}");
            }
        }

        // Show initial positions
        CreateMarker(level.theseusStartPosition, theseusMarkerColor, "Theseus Start");
        CreateMarker(level.minotaurStartPosition, minotaurMarkerColor, "Minotaur Start");

        yield return new WaitForSeconds(stepDelay);

        // Animate each move
        for (int i = 0; i < currentSolution.Count; i++)
        {
            var move = currentSolution[i];

            // Only clear position markers, keep wait markers
            ClearPositionMarkers();

            // Show move info
            string action = move.isWait ? "⏸ WAIT" : $"➜ Move {GetDirectionName(move.theseusFrom, move.theseusTo)}";
            Debug.Log($"Step {i + 1}/{currentSolution.Count}: {action}");

            // Show current positions (don't show Theseus marker if it's a wait - the yellow cube is already there)
            if (!move.isWait)
            {
                CreateMarker(move.theseusTo, theseusMarkerColor, $"T-{i + 1}");
            }

            CreateMarker(move.minotaurAfterMove2, minotaurMarkerColor, $"M-{i + 1}");

            // Draw paths up to this point
            DrawPartialSolution(i + 1);

            yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log("Animation complete!");
        isPlayingSolution = false;
        playRoutine = null;
    }

    private void DrawPartialSolution(int steps)
    {
        if (currentSolution == null || steps == 0) return;
        if (gridManager == null)
        {
            Debug.LogError("GridManager is null! Cannot draw partial solution.");
            return;
        }

        LevelData level = GetCurrentLevel();
        if (level == null) return;

        // Build partial paths
        List<Vector3> theseusPath = new List<Vector3>
        {
            gridManager.GridToWorldPos(level.theseusStartPosition)
        };

        List<Vector3> minotaurPath = new List<Vector3>
        {
            gridManager.GridToWorldPos(level.minotaurStartPosition)
        };

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
        if (gridManager == null)
        {
            Debug.LogError("GridManager is null! Cannot create marker.");
            return;
        }

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

    private void CreateWaitMarker(Vector2Int gridPos, string label)
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager is null! Cannot create wait marker.");
            return;
        }

        // Create a larger, yellow, pulsing marker for wait positions
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = label;
        marker.transform.position = gridManager.GridToWorldPos(gridPos);
        marker.transform.localScale = Vector3.one * waitMarkerSize;
        marker.transform.SetParent(transform);

        Renderer renderer = marker.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = waitMarkerColor;
        renderer.sortingOrder = 3;

        Destroy(marker.GetComponent<Collider>());

        // Add a simple rotation animation to make it stand out
        marker.AddComponent<WaitMarkerRotation>();

        waitMarkers.Add(marker); // Add to separate wait markers list
    }

    private void ClearMarkers()
    {
        ClearPositionMarkers();
        ClearWaitMarkers();
    }

    private void ClearPositionMarkers()
    {
        foreach (var marker in positionMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }

        positionMarkers.Clear();
    }

    private void ClearWaitMarkers()
    {
        foreach (var marker in waitMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }

        waitMarkers.Clear();
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
        return levelsManager?.GetCurrentLevelData();
    }
}