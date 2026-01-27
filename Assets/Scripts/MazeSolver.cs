using System.Collections.Generic;
using UnityEngine;

public class MazeSolver : MonoBehaviour
{
    private LevelData currentLevel;
    
    // Represents a complete game state with full history
    public class GameState
    {
        public Vector2Int theseusPos;
        public Vector2Int minotaurPos;
        public List<Move> moveHistory; // Full move sequence
        public int depth;
        
        public GameState(Vector2Int theseus, Vector2Int minotaur)
        {
            theseusPos = theseus;
            minotaurPos = minotaur;
            moveHistory = new List<Move>();
            depth = 0;
        }
        
        public GameState Clone()
        {
            GameState newState = new GameState(theseusPos, minotaurPos);
            newState.moveHistory = new List<Move>(moveHistory);
            newState.depth = depth;
            return newState;
        }
        
        public string GetStateKey()
        {
            return $"{theseusPos.x},{theseusPos.y}|{minotaurPos.x},{minotaurPos.y}";
        }
    }
    
    public class Move
    {
        public Vector2Int theseusFrom;
        public Vector2Int theseusTo;
        public Vector2Int minotaurFrom;
        public Vector2Int minotaurAfterMove1;
        public Vector2Int minotaurAfterMove2;
        public bool isWait;
        
        public Move(Vector2Int tFrom, Vector2Int tTo, Vector2Int mFrom, Vector2Int m1, Vector2Int m2, bool wait = false)
        {
            theseusFrom = tFrom;
            theseusTo = tTo;
            minotaurFrom = mFrom;
            minotaurAfterMove1 = m1;
            minotaurAfterMove2 = m2;
            isWait = wait;
        }
    }
    
    /// <summary>
    /// Solve the maze and return complete solution with all character positions
    /// </summary>
    public List<Move> SolveMaze(LevelData levelData)
    {
        currentLevel = levelData;
        
        Vector2Int start = levelData.theseusStartPosition;
        Vector2Int minotaurStart = levelData.minotaurStartPosition;
        Vector2Int exit = levelData.exitPosition;
        
        Debug.Log($"=== SOLVING MAZE ===");
        Debug.Log($"Theseus starts at: {start}");
        Debug.Log($"Minotaur starts at: {minotaurStart}");
        Debug.Log($"Exit at: {exit}");
        
        Queue<GameState> queue = new Queue<GameState>();
        HashSet<string> visited = new HashSet<string>();
        
        GameState initialState = new GameState(start, minotaurStart);
        queue.Enqueue(initialState);
        visited.Add(initialState.GetStateKey());
        
        int statesExplored = 0;
        int maxStates = 50000;
        
        while (queue.Count > 0 && statesExplored < maxStates)
        {
            statesExplored++;
            GameState current = queue.Dequeue();
            
            // Check if Theseus reached the exit
            if (current.theseusPos == exit)
            {
                Debug.Log($"✓ SOLUTION FOUND!");
                Debug.Log($"  Moves: {current.moveHistory.Count}");
                Debug.Log($"  States explored: {statesExplored}");
                PrintSolution(current.moveHistory);
                return current.moveHistory;
            }
            
            // Try all 5 possible actions (4 directions + wait)
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.zero // Wait
            };
            
            foreach (Vector2Int dir in directions)
            {
                GameState newState = TryMove(current, dir);
                
                if (newState != null)
                {
                    string stateKey = newState.GetStateKey();
                    
                    if (!visited.Contains(stateKey))
                    {
                        visited.Add(stateKey);
                        queue.Enqueue(newState);
                    }
                }
            }
            
            // Debug progress every 1000 states
            if (statesExplored % 1000 == 0)
            {
                Debug.Log($"Explored {statesExplored} states, queue size: {queue.Count}, depth: {current.depth}");
            }
        }
        
        Debug.LogWarning($"✗ NO SOLUTION FOUND after exploring {statesExplored} states");
        return null;
    }
    
    /// <summary>
    /// Try to make a move and return new state if valid (not caught)
    /// </summary>
    private GameState TryMove(GameState current, Vector2Int direction)
    {
        Vector2Int newTheseusPos;
        bool isWait = false;
        
        // Handle wait move
        if (direction == Vector2Int.zero)
        {
            newTheseusPos = current.theseusPos;
            isWait = true;
        }
        else
        {
            newTheseusPos = current.theseusPos + direction;
            
            // Check if move is valid for Theseus
            if (!IsValidMove(current.theseusPos, newTheseusPos))
                return null;
        }
        
        // Simulate first Minotaur move
        Vector2Int minotaurPos1 = SimulateMinotaurMove(current.minotaurPos, newTheseusPos);
        
        // Check if caught after first Minotaur move
        if (minotaurPos1 == newTheseusPos)
            return null;
        
        // Simulate second Minotaur move
        Vector2Int minotaurPos2 = SimulateMinotaurMove(minotaurPos1, newTheseusPos);
        
        // Check if caught after second Minotaur move
        if (minotaurPos2 == newTheseusPos)
            return null;
        
        // Create new valid state
        GameState newState = current.Clone();
        newState.theseusPos = newTheseusPos;
        newState.minotaurPos = minotaurPos2;
        newState.depth = current.depth + 1;
        
        // Record the complete move
        Move move = new Move(
            current.theseusPos,
            newTheseusPos,
            current.minotaurPos,
            minotaurPos1,
            minotaurPos2,
            isWait
        );
        
        newState.moveHistory.Add(move);
        
        return newState;
    }
    
    /// <summary>
    /// Simulate one Minotaur move toward Theseus
    /// </summary>
    private Vector2Int SimulateMinotaurMove(Vector2Int minotaurPos, Vector2Int theseusPos)
    {
        int dx = theseusPos.x - minotaurPos.x;
        int dy = theseusPos.y - minotaurPos.y;
        
        // Try horizontal first (Minotaur's priority)
        if (dx != 0)
        {
            Vector2Int direction = dx > 0 ? Vector2Int.right : Vector2Int.left;
            Vector2Int newPos = minotaurPos + direction;
            
            if (IsValidMove(minotaurPos, newPos))
                return newPos;
        }
        
        // Try vertical if horizontal failed
        if (dy != 0)
        {
            Vector2Int direction = dy > 0 ? Vector2Int.up : Vector2Int.down;
            Vector2Int newPos = minotaurPos + direction;
            
            if (IsValidMove(minotaurPos, newPos))
                return newPos;
        }
        
        // Can't move - Minotaur is trapped or blocked
        return minotaurPos;
    }
    
    /// <summary>
    /// Check if a move is valid (no walls blocking)
    /// </summary>
    private bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        // Check bounds
        if (to.x < 0 || to.x >= currentLevel.width || to.y < 0 || to.y >= currentLevel.height)
            return false;
        
        // Same position (wait) is valid
        if (from == to)
            return true;
        
        Vector2Int direction = to - from;
        
        // Get source cell
        CellData fromCell = currentLevel.GetCellData(from.x, from.y);
        if (fromCell == null) return false;
        
        // Check walls in source cell
        if (direction == Vector2Int.up && fromCell.wallUp) return false;
        if (direction == Vector2Int.down && fromCell.wallDown) return false;
        if (direction == Vector2Int.left && fromCell.wallLeft) return false;
        if (direction == Vector2Int.right && fromCell.wallRight) return false;
        
        // Get destination cell
        CellData toCell = currentLevel.GetCellData(to.x, to.y);
        if (toCell == null) return false;
        
        // Check walls in destination cell (entry walls)
        if (direction == Vector2Int.up && toCell.wallDown) return false;
        if (direction == Vector2Int.down && toCell.wallUp) return false;
        if (direction == Vector2Int.left && toCell.wallRight) return false;
        if (direction == Vector2Int.right && toCell.wallLeft) return false;
        
        return true;
    }
    
    /// <summary>
    /// Print the solution for debugging
    /// </summary>
    private void PrintSolution(List<Move> moves)
    {
        Debug.Log("=== SOLUTION PATH ===");
        
        for (int i = 0; i < moves.Count; i++)
        {
            Move m = moves[i];
            string action = m.isWait ? "WAIT" : $"Move {GetDirectionName(m.theseusFrom, m.theseusTo)}";
            
            Debug.Log($"Step {i + 1}: {action}");
            Debug.Log($"  Theseus: {m.theseusFrom} → {m.theseusTo}");
            Debug.Log($"  Minotaur: {m.minotaurFrom} → {m.minotaurAfterMove1} → {m.minotaurAfterMove2}");
        }
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
}