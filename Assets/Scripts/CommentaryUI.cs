using TMPro;
using UnityEngine;

public class CommentaryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI commentaryText;

    private void OnEnable()
    {
        GameEvents.OnNewLevelLoad += SetCommentaryText;
        GameEvents.OnGameStarted += GameStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnNewLevelLoad -= SetCommentaryText;
        GameEvents.OnGameStarted -= GameStarted;
    }

    private void SetCommentaryText(string text)
    {
        commentaryText.text = text;
    }

    private void GameStarted()
    {
        transform.localScale = Vector3.one;
    }
}