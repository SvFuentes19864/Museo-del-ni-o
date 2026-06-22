using UnityEngine;
using UnityEngine.UI;

public class VirtualCursorF4 : MonoBehaviour
{
    [Header("Referencias")]
    public HandTrackerF4 handTrackerF4;

    public RectTransform cursorImageF4;

    [Header("Calibración")]
    public float offsetXF4 = 0f;
    public float offsetYF4 = 0f;

    public float escalaXF4 = 1f;
    public float escalaYF4 = 1f;

    void Update()
    {
        if (handTrackerF4 == null)
            return;

        Vector2 normalized =
            handTrackerF4.handPositionNormalizedF4;

        float screenX =
            ((normalized.x - 0.5f) * escalaXF4 + 0.5f)
            * Screen.width
            + offsetXF4;

        float screenY =
            ((0.5f - normalized.y) * escalaYF4 + 0.5f)
            * Screen.height
            + offsetYF4;

        cursorImageF4.position =
            new Vector3(
                screenX,
                screenY,
                0f
            );
    }
}