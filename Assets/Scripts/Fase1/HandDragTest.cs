using UnityEngine;

public class HandDragTest : MonoBehaviour
{
    public HandTracker handTracker;

    public Camera cam;

    void Update()
    {
        if (handTracker == null)
            return;

        Vector2 p =
            handTracker.handPositionNormalized;

        Vector3 screenPos =
            new Vector3(
                p.x * Screen.width,
                (1f - p.y) * Screen.height,
                10f
            );

        Vector3 worldPos =
            cam.ScreenToWorldPoint(screenPos);

        transform.position =
            new Vector3(
                worldPos.x,
                worldPos.y,
                0f
            );
    }
}