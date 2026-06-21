using UnityEngine;

public class HandDebug : MonoBehaviour
{
    public HandTracker handTracker;

    private bool estadoAnterior;

    void Update()
    {
        if (handTracker == null)
            return;

        if (handTracker.handPressed != estadoAnterior)
        {
            estadoAnterior = handTracker.handPressed;

            Debug.Log(
                "CLICK: " +
                handTracker.handPressed
            );
        }
    }
}