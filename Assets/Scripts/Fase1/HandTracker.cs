using UnityEngine;
using OrbbecUnity;

public class HandTracker : MonoBehaviour
{
    [Header("Orbbec")]
    public OrbbecPipelineFrameSource frameSource;

    [Header("Objeto a mover")]
    public Transform handSphere;

    [Header("Detección")]
    public int saltoPixeles = 4;

    [Tooltip("Píxeles aceptados desde la profundidad mínima encontrada")]
    public ushort margenProfundidad = 100;

    [Header("Escala Mundo")]
    public float escalaX = 4f;
    public float escalaY = 3f;

    [Header("Suavizado")]
    public float velocidadSuavizado = 10f;

    [HideInInspector]
    public Vector2 handPositionNormalized;

    [HideInInspector]
    public float handDepth;

    [HideInInspector]
    public bool handPressed;

    private Vector3 posicionActual;

    void Update()
    {
        if (frameSource == null)
            return;

        var depthFrame = frameSource.GetDepthFrame();

        if (depthFrame == null)
            return;

        if (depthFrame.data == null)
            return;

        int width = depthFrame.width;
        int height = depthFrame.height;

        ushort profundidadMinima = ushort.MaxValue;

        // Buscar profundidad mínima
        for (int y = 0; y < height; y += saltoPixeles)
        {
            for (int x = 0; x < width; x += saltoPixeles)
            {
                int index = (y * width + x) * 2;

                if (index + 1 >= depthFrame.data.Length)
                    continue;

                ushort depth =
                    System.BitConverter.ToUInt16(
                        depthFrame.data,
                        index
                    );

                if (depth == 0)
                    continue;

                if (depth < profundidadMinima)
                {
                    profundidadMinima = depth;
                }
            }
        }

        if (profundidadMinima == ushort.MaxValue)
            return;

        ushort profundidadMaxima =
            (ushort)(profundidadMinima + margenProfundidad);

        long sumaX = 0;
        long sumaY = 0;
        long sumaDepth = 0;

        int contador = 0;

        // Calcular centro de la región cercana
        for (int y = 0; y < height; y += saltoPixeles)
        {
            for (int x = 0; x < width; x += saltoPixeles)
            {
                int index = (y * width + x) * 2;

                if (index + 1 >= depthFrame.data.Length)
                    continue;

                ushort depth =
                    System.BitConverter.ToUInt16(
                        depthFrame.data,
                        index
                    );

                if (depth == 0)
                    continue;

                if (depth >= profundidadMinima &&
                    depth <= profundidadMaxima)
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

        // Posición normalizada para el cursor virtual
        handPositionNormalized = new Vector2(
            promedioX / width,
            promedioY / height
        );

        handDepth = promedioDepth;

        if (handDepth < 105)
        {
            handPressed = true;
        }
        else
        {
            handPressed = false;
        }

        float worldX =
            ((promedioX / width) - 0.5f) * escalaX;

        float worldY =
            ((promedioY / height) - 0.5f) * escalaY;

        float worldZ =
            promedioDepth / 1000f;

        Vector3 posicionObjetivo =
            new Vector3(
                worldX,
                -worldY,
                worldZ
            );

        posicionActual = Vector3.Lerp(
            posicionActual,
            posicionObjetivo,
            velocidadSuavizado * Time.deltaTime
        );

        handSphere.position = posicionActual;
    }
}