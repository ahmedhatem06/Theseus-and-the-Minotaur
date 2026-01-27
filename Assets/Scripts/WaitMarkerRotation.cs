using UnityEngine;

public class WaitMarkerRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    
    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}