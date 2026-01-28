using UnityEngine;

public class GameStateController : MonoBehaviour
{
    private Theseus theseus;
    private Minotaur minotaur;
    private GridManager gridManager;

    public void Initialize(GridManager grid, Theseus t, Minotaur m)
    {
        gridManager = grid;
        theseus = t;
        minotaur = m;
    }

    private void OnEnable()
    {
        GameEvents.OnTurnCompleted += CheckGameState;
    }

    private void OnDisable()
    {
        GameEvents.OnTurnCompleted -= CheckGameState;
    }

    private void CheckGameState()
    {
        if (theseus.GridPos == minotaur.GridPos)
        {
            GameEvents.GameLost();
            Debug.Log("Game Over | Lost");
        }
        else if (theseus.GridPos == gridManager.ExitPos)
        {
            GameEvents.GameWon();
            Debug.Log("Game Over | Won");
        }
    }
}