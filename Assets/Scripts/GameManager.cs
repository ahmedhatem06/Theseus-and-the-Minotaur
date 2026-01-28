using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TurnHistory turnHistory;

    [SerializeField] private TurnSystem turnSystem;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private LevelsManager levelsManager;
    [SerializeField] private SolutionVisualizer solutionVisualizer;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private Camera cam;

    private void Start()
    {
        gridManager.Initialize(playerSpawner, turnHistory, turnSystem, gameStateController, cam);
        turnSystem.Initialize(turnHistory);
        solutionVisualizer.Initialize(levelsManager);
    }
}