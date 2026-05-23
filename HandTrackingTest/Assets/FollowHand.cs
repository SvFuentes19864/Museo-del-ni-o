using UnityEngine;

public class FollowHand : MonoBehaviour
{
    public Transform hand;

    void Update()
    {
        if (hand != null)
        {
            transform.position = hand.position;
        }
    }
}