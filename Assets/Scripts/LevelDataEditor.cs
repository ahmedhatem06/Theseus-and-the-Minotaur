#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private Vector2 scrollPosition;
    private const float cellSize = 80f;
    private const float wallThickness = 8f;
    private const float spacing = 2f;

    public override void OnInspectorGUI()
    {
        LevelData levelData = (LevelData)target;

        // Draw default inspector for basic fields
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);
        levelData.levelName = EditorGUILayout.TextField("Level Name", levelData.levelName);
        levelData.levelNumber = EditorGUILayout.IntField("Level Number", levelData.levelNumber);
        levelData.levelDescription = EditorGUILayout.TextField("Level Description", levelData.levelDescription);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Size", EditorStyles.boldLabel);

        int newWidth = EditorGUILayout.IntField("Width", levelData.width);
        int newHeight = EditorGUILayout.IntField("Height", levelData.height);

        if (newWidth != levelData.width || newHeight != levelData.height)
        {
            levelData.width = Mathf.Max(1, newWidth);
            levelData.height = Mathf.Max(1, newHeight);
            levelData.OnValidate();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Starting Positions", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        Vector2Int newTheseusPos = EditorGUILayout.Vector2IntField("Theseus Start", levelData.theseusStartPosition);
        if (EditorGUI.EndChangeCheck())
        {
            levelData.theseusStartPosition = newTheseusPos;
            EditorUtility.SetDirty(levelData);
        }

        EditorGUI.BeginChangeCheck();
        Vector2Int newMinotaurPos = EditorGUILayout.Vector2IntField("Minotaur Start", levelData.minotaurStartPosition);
        if (EditorGUI.EndChangeCheck())
        {
            levelData.minotaurStartPosition = newMinotaurPos;
            EditorUtility.SetDirty(levelData);
        }

        EditorGUI.BeginChangeCheck();
        Vector2Int newExitPos = EditorGUILayout.Vector2IntField("Exit Position", levelData.exitPosition);
        if (EditorGUI.EndChangeCheck())
        {
            levelData.exitPosition = newExitPos;
            EditorUtility.SetDirty(levelData);
            Repaint(); // Force repaint to show visual changes
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(levelData);
        }

        EditorGUILayout.Space();

        // Helper buttons
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Border Walls"))
        {
            levelData.CreateBorderWalls();
            EditorUtility.SetDirty(levelData);
        }

        if (GUILayout.Button("Clear All Walls"))
        {
            levelData.ClearAllWalls();
            EditorUtility.SetDirty(levelData);
        }

        if (GUILayout.Button("Set Exit"))
        {
            levelData.SetExitAtPosition();
            EditorUtility.SetDirty(levelData);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Click on walls to toggle them. Green cell = Exit. Red dot = Theseus. Black dot = Minotaur.",
            MessageType.Info);

        // Draw grid editor
        DrawGridEditor(levelData);
    }

    private void DrawGridEditor(LevelData levelData)
    {
        if (levelData.cells == null)
        {
            levelData.OnValidate();
        }

        EditorGUILayout.Space();
        scrollPosition =
            EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(300), GUILayout.MaxHeight(600));

        GUILayout.BeginVertical();

        // Draw from top to bottom (reverse y for visual clarity - y increases upward in game)
        for (int y = levelData.height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();

            for (int x = 0; x < levelData.width; x++)
            {
                DrawCellVisual(levelData, x, y);
                GUILayout.Space(spacing);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);
        }

        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DrawCellVisual(LevelData levelData, int x, int y)
    {
        CellData cellData = levelData.GetCellData(x, y);
        if (cellData == null) return;

        Rect cellRect = GUILayoutUtility.GetRect(cellSize, cellSize);

        // Draw cell background
        Color cellColor = cellData.hasExit ? new Color(0.5f, 1f, 0.5f) : new Color(0.9f, 0.9f, 0.9f);
        EditorGUI.DrawRect(cellRect, cellColor);

        // Draw walls
        Color wallColor = new Color(0.55f, 0.27f, 0.07f); // Brown color

        // Top wall (horizontal)
        if (cellData.wallUp)
        {
            Rect wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width, wallThickness);
            EditorGUI.DrawRect(wallRect, wallColor);
        }

        // Bottom wall (horizontal)
        if (cellData.wallDown)
        {
            Rect wallRect = new Rect(cellRect.x, cellRect.y + cellRect.height - wallThickness, cellRect.width,
                wallThickness);
            EditorGUI.DrawRect(wallRect, wallColor);
        }

        // Left wall (vertical)
        if (cellData.wallLeft)
        {
            Rect wallRect = new Rect(cellRect.x, cellRect.y, wallThickness, cellRect.height);
            EditorGUI.DrawRect(wallRect, wallColor);
        }

        // Right wall (vertical)
        if (cellData.wallRight)
        {
            Rect wallRect = new Rect(cellRect.x + cellRect.width - wallThickness, cellRect.y, wallThickness,
                cellRect.height);
            EditorGUI.DrawRect(wallRect, wallColor);
        }

        // Draw character indicators
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 20;
        labelStyle.fontStyle = FontStyle.Bold;

        string label = "";
        if (levelData.theseusStartPosition == new Vector2Int(x, y))
        {
            labelStyle.normal.textColor = Color.red;
            label = "T";
        }
        else if (levelData.minotaurStartPosition == new Vector2Int(x, y))
        {
            labelStyle.normal.textColor = Color.black;
            label = "M";
        }

        if (!string.IsNullOrEmpty(label))
        {
            GUI.Label(cellRect, label, labelStyle);
        }

        // Draw coordinate label (small, bottom-right)
        GUIStyle coordStyle = new GUIStyle(GUI.skin.label);
        coordStyle.fontSize = 8;
        coordStyle.alignment = TextAnchor.LowerRight;
        coordStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(cellRect.x, cellRect.y, cellRect.width - 2, cellRect.height - 2), $"{x},{y}", coordStyle);

        // Handle clicks to toggle walls and exit
        Event e = Event.current;
        if (e.type == EventType.MouseDown && cellRect.Contains(e.mousePosition))
        {
            Vector2 localClick = e.mousePosition - cellRect.position;

            // Determine what was clicked

            // Check top wall
            if (localClick.y < wallThickness * 2)
            {
                cellData.wallUp = !cellData.wallUp;
            }
            // Check bottom wall
            else if (localClick.y > cellRect.height - wallThickness * 2)
            {
                cellData.wallDown = !cellData.wallDown;
            }
            // Check left wall
            else if (localClick.x < wallThickness * 2)
            {
                cellData.wallLeft = !cellData.wallLeft;
            }
            // Check right wall
            else if (localClick.x > cellRect.width - wallThickness * 2)
            {
                cellData.wallRight = !cellData.wallRight;
            }
            // Clicked center - set as exit and update exit position
            else
            {
                // Clear all exits first
                for (int i = 0; i < levelData.cells.Length; i++)
                {
                    if (levelData.cells[i] != null)
                    {
                        levelData.cells[i].hasExit = false;
                    }
                }

                // Set this cell as exit
                cellData.hasExit = true;

                // Update the exit position in the level data
                levelData.exitPosition = new Vector2Int(x, y);
            }

            EditorUtility.SetDirty(levelData);
            e.Use();
        }
    }
}
#endif