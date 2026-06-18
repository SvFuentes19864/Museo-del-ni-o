using UnityEngine;

public class InfiniteRotateY : MonoBehaviour
{
    [SerializeField] public float rotationSpeed = -30f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}