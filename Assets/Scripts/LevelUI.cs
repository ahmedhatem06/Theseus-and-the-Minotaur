using UnityEngine;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI levelText;

    private void OnEnable()
    {
        GameEvents.OnLevelLoaded += LevelLoaded;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelLoaded -= LevelLoaded;
    }

    private void LevelLoaded(int currentLevelIndex)
    {
        levelText.text = (currentLevelIndex + 1).ToString();
    }
}