using UnityEngine;

public class HandController : MonoBehaviour
{
    public Vector3 handPosition;

    void Start()
    {
        handPosition = new Vector3(-5, 0, -5);
    }

    void Update()
    {
        transform.position = handPosition;
    }
}