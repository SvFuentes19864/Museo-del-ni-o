using UnityEngine;

public class HandInputSimulator : MonoBehaviour
{
    public HandController hand;

    void Update()
    {
        float x = Mathf.Sin(Time.time) * 2f;
        float z = Mathf.Cos(Time.time) * 2f;

        Vector3 simulatedHand = new Vector3(x, 0, z);

        hand.handPosition = simulatedHand;
    }
}