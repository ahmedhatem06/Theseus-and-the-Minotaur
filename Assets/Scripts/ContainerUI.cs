using UnityEngine;

public class ContainerUI : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameEvents.GameStarted();
            gameObject.SetActive(false);
        }
    }
}