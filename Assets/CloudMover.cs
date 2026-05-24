using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public float speed = 0.2f;
    public Vector3 direction = Vector3.right;

    [Header("Floating")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // movimiento horizontal
        transform.position += direction * speed * Time.deltaTime;

        // flotación suave
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + yOffset,
            transform.position.z
        );
    }
}