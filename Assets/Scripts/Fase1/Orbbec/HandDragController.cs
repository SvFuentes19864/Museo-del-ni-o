using UnityEngine;

public class HandDragController : MonoBehaviour
{
    public HandTracker handTracker;

    public Camera cam;

    public float distanciaSeleccion = 1.5f;

    private HandDraggable objetoSeleccionado;

    void Update()
    {
        if (handTracker == null || cam == null)
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

        Vector3 posicionMano =
            new Vector3(
                worldPos.x,
                0f,
                worldPos.y
            );

        if (handTracker.handPressed)
        {
            if (objetoSeleccionado == null)
            {
                BuscarObjeto(posicionMano);
            }

            if (objetoSeleccionado != null)
            {
                objetoSeleccionado.transform.position =
                    new Vector3(
                        posicionMano.x,
                        objetoSeleccionado.transform.position.y,
                        posicionMano.z
                    );
            }
        }
        else
        {
            objetoSeleccionado = null;
        }
    }

    void BuscarObjeto(Vector3 posicionMano)
    {
        HandDraggable[] objetos =
            FindObjectsByType<HandDraggable>(
                FindObjectsSortMode.None
            );

        float mejorDistancia =
            distanciaSeleccion;

        HandDraggable mejorObjeto = null;

        foreach (HandDraggable obj in objetos)
        {
            float distancia =
                Vector3.Distance(
                    obj.transform.position,
                    posicionMano
                );

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejorObjeto = obj;
            }
        }

        objetoSeleccionado = mejorObjeto;
    }
}