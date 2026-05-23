using UnityEngine;

public class HoverObject : MonoBehaviour
{
    public Transform handCursor;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float distance = Vector3.Distance(
            transform.position,
            handCursor.position
        );

        Debug.Log(distance);

        if (distance < 0.3f)
        {
            rend.material.color = Color.green;
        }
        else
        {
            rend.material.color = Color.white;
        }
    }
}