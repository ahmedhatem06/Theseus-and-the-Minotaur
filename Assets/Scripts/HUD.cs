using UnityEngine;

public class HUD : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnGameStarted += GameStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= GameStarted;
    }

    private void GameStarted()
    {
        transform.localScale = Vector3.one;
    }
}