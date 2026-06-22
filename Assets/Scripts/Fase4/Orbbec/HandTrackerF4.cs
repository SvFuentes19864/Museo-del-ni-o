using UnityEngine;
using OrbbecUnity;
using UnityEngine.UI;

public class HandTrackerF4 : MonoBehaviour
{

    [Header("Calibración")]
    public float umbralPresionado = 105f;

    [Header("Orbbec")]
    public OrbbecPipelineFrameSource frameSource;

    [Header("Objeto actual F4")]
    public Transform objetoActualF4;

    [Tooltip("Offset visual para centrar el objeto")]
    public Vector3 offsetVisual3DF4 = Vector3.zero;

    [Header("Interacción 2D (Canvas)")]
    public RectTransform orbe2DF4;
    public Canvas canvasPrincipalF4;

    [Header("Detección")]
    public int saltoPixelesF4 = 4;
    public ushort margenProfundidadF4 = 100;

    [Header("Suavizado")]
    public float velocidadSuavizadoF4 = 30f;

    [HideInInspector]
    public Vector2 handPositionNormalizedF4;

    [HideInInspector]
    public float handDepthF4;

    [HideInInspector]
    public bool handPressedF4;

    private float alturaFijaYF4;

    private bool lastHandPressedF4 = false;

    public void CambiarObjetoF4(
        Transform nuevoObjeto
    )
    {
        objetoActualF4 = nuevoObjeto;

        if (objetoActualF4 != null)
        {
            alturaFijaYF4 =
                objetoActualF4.position.y;
        }
    }

    void Start()
    {
        if (objetoActualF4 != null)
        {
            alturaFijaYF4 =
                objetoActualF4.position.y;
        }
    }

    void Update()
    {
        if (frameSource == null)
            return;

        var depthFrame =
            frameSource.GetDepthFrame();

        if (
            depthFrame == null ||
            depthFrame.data == null
        )
            return;

        int width = depthFrame.width;
        int height = depthFrame.height;

        ushort profundidadMinima =
            ushort.MaxValue;

        for (
            int y = 0;
            y < height;
            y += saltoPixelesF4
        )
        {
            for (
                int x = 0;
                x < width;
                x += saltoPixelesF4
            )
            {
                int index =
                    (y * width + x) * 2;

                if (
                    index + 1 >=
                    depthFrame.data.Length
                )
                    continue;

                ushort depth =
                    System.BitConverter
                        .ToUInt16(
                            depthFrame.data,
                            index
                        );

                if (depth == 0)
                    continue;

                if (
                    depth <
                    profundidadMinima
                )
                {
                    profundidadMinima =
                        depth;
                }
            }
        }

        if (
            profundidadMinima ==
            ushort.MaxValue
        )
            return;

        ushort profundidadMaxima =
            (ushort)(
                profundidadMinima +
                margenProfundidadF4
            );

        long sumaX = 0;
        long sumaY = 0;
        long sumaDepth = 0;

        int contador = 0;

        for (
            int y = 0;
            y < height;
            y += saltoPixelesF4
        )
        {
            for (
                int x = 0;
                x < width;
                x += saltoPixelesF4
            )
            {
                int index =
                    (y * width + x) * 2;

                if (
                    index + 1 >=
                    depthFrame.data.Length
                )
                    continue;

                ushort depth =
                    System.BitConverter
                        .ToUInt16(
                            depthFrame.data,
                            index
                        );

                if (depth == 0)
                    continue;

                if (
                    depth >= profundidadMinima &&
                    depth <= profundidadMaxima
                )
                {
                    sumaX += x;
                    sumaY += y;
                    sumaDepth += depth;
                    contador++;
                }
            }
        }

        if (contador == 0)
            return;

        float promedioX =
            (float)sumaX / contador;

        float promedioY =
            (float)sumaY / contador;

        float promedioDepth =
            (float)sumaDepth / contador;

        handPositionNormalizedF4 =
            new Vector2(
                promedioX / width,
                promedioY / height
            );

        handDepthF4 =
            promedioDepth;

        handPressedF4 = handDepthF4 < umbralPresionado;

        HandDraggableF4 draggable =
            null;

        if (objetoActualF4 != null)
        {
            draggable =
                objetoActualF4
                    .GetComponent
                    <HandDraggableF4>();
        }

        if (
            lastHandPressedF4 &&
            !handPressedF4
        )
        {
            if (
                draggable != null &&
                !draggable.yaColocadoF4
            )
            {
                if (
                    draggable
                        .PuedeColocarseF4()
                )
                {
                    draggable.ColocarF4();
                }
            }
        }

        lastHandPressedF4 =
            handPressedF4;

        Vector3 posicionPantallaRaw =
            new Vector3(
                handPositionNormalizedF4.x *
                Screen.width,

                (1f -
                 handPositionNormalizedF4.y)
                * Screen.height,

                0f
            );

        if (
            orbe2DF4 != null &&
            canvasPrincipalF4 != null
        )
        {
            Vector2 posicionLocalCanvas;

            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasPrincipalF4
                        .transform
                        as RectTransform,

                    posicionPantallaRaw,

                    canvasPrincipalF4
                        .renderMode ==
                    RenderMode
                        .ScreenSpaceOverlay
                            ? null
                            : Camera.main,

                    out posicionLocalCanvas
                );

            if (handPressedF4)
            {
                orbe2DF4.localPosition =
                    Vector2.Lerp(
                        orbe2DF4.localPosition,
                        posicionLocalCanvas,
                        velocidadSuavizadoF4 *
                        Time.deltaTime
                    );
            }
        }

        if (
            objetoActualF4 != null &&
            Camera.main != null &&
            orbe2DF4 != null
        )
        {
            if (
                draggable == null ||
                !draggable.yaColocadoF4
            )
            {
                Vector3 posicionVisualUI =
                    orbe2DF4.position;

                Plane planoPiso =
                    new Plane(
                        Vector3.up,
                        new Vector3(
                            0,
                            alturaFijaYF4,
                            0
                        )
                    );

                Ray rayoDesdeCamara =
                    Camera.main
                        .ScreenPointToRay(
                            posicionVisualUI
                        );

                if (
                    planoPiso.Raycast(
                        rayoDesdeCamara,
                        out float distanciaImpacto
                    )
                )
                {
                    Vector3 puntoObjetivo3D =
                        rayoDesdeCamara
                            .GetPoint(
                                distanciaImpacto
                            )
                        + offsetVisual3DF4;

                    objetoActualF4.position =
                        puntoObjetivo3D;
                }
            }
        }
    }
}