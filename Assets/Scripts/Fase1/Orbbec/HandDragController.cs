using System.Collections.Generic;
using UnityEngine;

public class HandDragController : MonoBehaviour
{
    [Header("Referencias")]
    public HandTracker handTracker;
    public Camera cam;
    public RectTransform orbe2D;

    [Header("Objetos móviles")]
    [Tooltip("Lista de objetos que el usuario puede arrastrar")]
    public List<HandDraggable> objetosMoviles = new();

    [Header("Detección visual (píxeles de pantalla)")]
    [Tooltip("Qué tan cerca en pantalla debe estar el orbe del objeto para poder arrastrarlo")]
    public float radioDeteccionPantalla = 100f;

    // Objeto que se está manipulando en este momento (solo uno a la vez)
    private HandDraggable objetoSeleccionado;

    void Update()
    {
        if (handTracker == null || cam == null)
            return;

        Vector3 posicionOrbe = OrbeEnPantalla();

        if (handTracker.handPressed)
        {
            // Solo buscar si no hay ninguno seleccionado (un objeto a la vez)
            if (objetoSeleccionado == null)
                BuscarObjetoVisual(posicionOrbe);

            if (objetoSeleccionado != null)
                MoverObjetoXZ(posicionOrbe);
        }
        else
        {
            objetoSeleccionado = null;
        }
    }

    Vector3 OrbeEnPantalla()
    {
        if (orbe2D != null)
            return orbe2D.position;

        return new Vector3(
            handTracker.handPositionNormalized.x * Screen.width,
            (1f - handTracker.handPositionNormalized.y) * Screen.height,
            0f
        );
    }

    void BuscarObjetoVisual(Vector3 posicionOrbeEnPantalla)
    {
        float mejorDistancia = radioDeteccionPantalla;
        HandDraggable mejorObjeto = null;

        foreach (HandDraggable obj in objetosMoviles)
        {
            if (obj == null || obj.yaColocado) continue;

            Vector3 posEnPantalla = cam.WorldToScreenPoint(obj.transform.position);

            if (posEnPantalla.z <= 0f) continue;

            float distancia2D = Vector2.Distance(
                new Vector2(posicionOrbeEnPantalla.x, posicionOrbeEnPantalla.y),
                new Vector2(posEnPantalla.x, posEnPantalla.y)
            );

            if (distancia2D < mejorDistancia)
            {
                mejorDistancia = distancia2D;
                mejorObjeto = obj;
            }
        }

        objetoSeleccionado = mejorObjeto;
    }

    void MoverObjetoXZ(Vector3 posicionOrbeEnPantalla)
    {
        Ray rayo = cam.ScreenPointToRay(posicionOrbeEnPantalla);

        float alturaY = objetoSeleccionado.transform.position.y;
        Plane planoHorizontal = new Plane(Vector3.up, new Vector3(0f, alturaY, 0f));

        if (planoHorizontal.Raycast(rayo, out float distancia))
        {
            Vector3 puntoMundo = rayo.GetPoint(distancia);
            objetoSeleccionado.transform.position = new Vector3(
                puntoMundo.x,
                alturaY,
                puntoMundo.z
            );
        }
    }
}
