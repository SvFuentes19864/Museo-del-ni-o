using UnityEngine;
using UnityEngine.UI;

public class VirtualCursor : MonoBehaviour
{
    [Header("Referencias")]
    public HandTracker handTracker;

    public RectTransform cursorImage;

    [Header("Calibración")]
    public float offsetX = 0f;
    public float offsetY = 0f;

    public float escalaX = 1f;
    public float escalaY = 1f;

    private bool estadoAnterior;

    void Update()
    {
        if (handTracker == null)
            return;

        Vector2 normalized =
            handTracker.handPositionNormalized;

        float screenX =
            ((normalized.x - 0.5f) * escalaX + 0.5f)
            * Screen.width
            + offsetX;

        float screenY =
            ((0.5f - normalized.y) * escalaY + 0.5f)
            * Screen.height
            + offsetY;

        cursorImage.position =
            new Vector3(
                screenX,
                screenY,
                0f
            );

        if (
            handTracker.handPressed &&
            !estadoAnterior
        )
        {
            estadoAnterior = true;

            Debug.Log("CLICK VIRTUAL");

            Ray ray =
                Camera.main.ScreenPointToRay(
                    new Vector3(
                        screenX,
                        screenY,
                        0f
                    )
                );

            RaycastHit hit;

            if (
                Physics.Raycast(
                    ray,
                    out hit,
                    100f
                )
            )
            {
                Debug.Log(
                    "OBJETO DETECTADO: " +
                    hit.collider.name
                );

                HandSelectable selectable =
                    hit.collider.GetComponent<HandSelectable>();

                if (selectable != null)
                {
                    selectable.seleccionado = true;

                    Debug.Log(
                        "SELECCIONADO: " +
                        hit.collider.name
                    );
                }

                ClickTest clickTest =
                    hit.collider.GetComponent<ClickTest>();

                if (clickTest != null)
                {
                    clickTest.ClickRecibido();
                }
            }
        }

        if (
            !handTracker.handPressed &&
            estadoAnterior
        )
        {
            estadoAnterior = false;

            Debug.Log("RELEASE VIRTUAL");
        }
    }
}