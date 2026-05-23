using UnityEngine;

public class CursorFollow : MonoBehaviour
{
    public HandController hand;

    void Update()
    {
        transform.position = hand.handPosition;
    }
}