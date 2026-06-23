using UnityEngine;
using OrbbecUnity;
using UnityEngine.UI;

public class HandTracker : MonoBehaviour
{

    [Header("Calibración")]
    public float umbralPresionado = 105f;

    [Header("Orbbec")]
    public OrbbecPipelineFrameSource frameSource;

    [Header("Objeto 3D a mover")]
    public Transform handSphere;
    [Tooltip("Usa esto si el cubo necesita un pequeño empujón en X o Z para verse centrado debajo del cursor")]
    public Vector3 offsetVisual3D = Vector3.zero;

    [Header("Interacción 2D (Canvas)")]
    public RectTransform orbe2D;
    public Canvas canvasPrincipal;

    [Header("Detección")]
    public int saltoPixeles = 4;
    public ushort margenProfundidad = 100;

    [Header("Suavizado")]
    public float velocidadSuavizado = 30f;

    [HideInInspector]
    public Vector2 handPositionNormalized;
    [HideInInspector]
    public float handDepth;
    [HideInInspector]
    public bool handPressed;

    // Altura fija para bloquear el eje Y
    private float alturaFijaY;
    
    // Variable para detectar el momento en que se "suelta" el objeto
    private bool lastHandPressed = false;

    void Start()
    {
        if (handSphere != null)
        {
            alturaFijaY = handSphere.position.y;
        }
    }

    void Update()
    {
        if (frameSource == null) return;
        var depthFrame = frameSource.GetDepthFrame();
        if (depthFrame == null || depthFrame.data == null) return;

        int width = depthFrame.width;
        int height = depthFrame.height;
        ushort profundidadMinima = ushort.MaxValue;

        // Buscar profundidad mínima
        for (int y = 0; y < height; y += saltoPixeles)
        {
            for (int x = 0; x < width; x += saltoPixeles)
            {
                int index = (y * width + x) * 2;
                if (index + 1 >= depthFrame.data.Length) continue;

                ushort depth = System.BitConverter.ToUInt16(depthFrame.data, index);
                if (depth == 0) continue;
                if (depth < profundidadMinima) profundidadMinima = depth;
            }
        }

        if (profundidadMinima == ushort.MaxValue) return;

        ushort profundidadMaxima = (ushort)(profundidadMinima + margenProfundidad);
        long sumaX = 0, sumaY = 0, sumaDepth = 0;
        int contador = 0;

        // Calcular centro
        for (int y = 0; y < height; y += saltoPixeles)
        {
            for (int x = 0; x < width; x += saltoPixeles)
            {
                int index = (y * width + x) * 2;
                if (index + 1 >= depthFrame.data.Length) continue;

                ushort depth = System.BitConverter.ToUInt16(depthFrame.data, index);
                if (depth == 0) continue;

                if (depth >= profundidadMinima && depth <= profundidadMaxima)
                {
                    sumaX += x; sumaY += y; sumaDepth += depth;
                    contador++;
                }
            }
        }

        if (contador == 0) return;

        float promedioX = (float)sumaX / contador;
        float promedioY = (float)sumaY / contador;
        float promedioDepth = (float)sumaDepth / contador;

        // Posición normalizada de la mano
        handPositionNormalized = new Vector2(promedioX / width, promedioY / height);
        handDepth = promedioDepth;
        handPressed = handDepth < umbralPresionado;

        

        // ---------------------------------------------------------
        // LÓGICA DE COLOCACIÓN (INTEGRACIÓN)
        // ---------------------------------------------------------
        HandDraggable draggable = null;
        if (handSphere != null)
        {
            draggable = handSphere.GetComponent<HandDraggable>();
        }

        // Si en el frame anterior estaba presionado y ahora no, significa que SOLTAMOS (Drop)
        if (lastHandPressed && !handPressed)
        {
            if (draggable != null && !draggable.yaColocado)
            {
                if (draggable.PuedeColocarse())
                {
                    draggable.Colocar();
                }
            }
        }

        // Actualizamos el estado para el siguiente frame
        lastHandPressed = handPressed;

        // ---------------------------------------------------------
        // 1. LÓGICA 2D: Mover Cursor de Canvas
        // ---------------------------------------------------------
        Vector3 posicionPantallaRaw = new Vector3(
            handPositionNormalized.x * Screen.width,
            (1f - handPositionNormalized.y) * Screen.height,
            0f
        );

        if (orbe2D != null && canvasPrincipal != null)
        {
            Vector2 posicionLocalCanvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasPrincipal.transform as RectTransform,
                posicionPantallaRaw,
                canvasPrincipal.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out posicionLocalCanvas
            );

            // Mueve el cursor 2D si estamos presionando
            if (handPressed)
            {
                orbe2D.localPosition = Vector2.Lerp(
                    orbe2D.localPosition,
                    posicionLocalCanvas,
                    velocidadSuavizado * Time.deltaTime
                );
            }
        }

        // ---------------------------------------------------------
        // 2. LÓGICA 3D: Hacer match y bloquear si ya se colocó
        // ---------------------------------------------------------
        if (handSphere != null && Camera.main != null && orbe2D != null)
        {
            // Verificamos que NO esté colocado antes de seguir moviéndolo
            if (draggable == null || !draggable.yaColocado)
            {
                Vector3 posicionVisualUI = orbe2D.position;
                Plane planoPiso = new Plane(Vector3.up, new Vector3(0, alturaFijaY, 0));
                Ray rayoDesdeCamara = Camera.main.ScreenPointToRay(posicionVisualUI);

                if (planoPiso.Raycast(rayoDesdeCamara, out float distanciaImpacto))
                {
                    Vector3 puntoObjetivo3D = rayoDesdeCamara.GetPoint(distanciaImpacto) + offsetVisual3D;
                    handSphere.position = puntoObjetivo3D;
                }
            }
        }
    }
}