using UnityEngine;
using UnityEngine.Events;

public class HandDraggableF4 : MonoBehaviour
{
    [Header("GameManager")]
    public GameManager gameManager;

    [Header("Colocación")]
    public bool yaColocadoF4 = false;

    [Header("Zona correcta")]
    public Transform zonaCorrectaF4;

    [Header("Distancia de snap")]
    public float distanciaSnapF4 = 3f;

    [Header("Evento al colocar")]
    public UnityEvent onPlacedF4;

    private Vector3 offsetCentroF4;

    void Start()
    {
        Renderer rendererPrincipal =
            GetComponent<Renderer>();

        if (rendererPrincipal != null)
        {
            Bounds bounds =
                rendererPrincipal.bounds;

            offsetCentroF4 = new Vector3(
                transform.position.x - bounds.center.x,
                0,
                transform.position.z - bounds.center.z
            );
        }
    }

    public bool PuedeColocarseF4()
    {
        if (zonaCorrectaF4 == null)
            return false;

        float distancia =
            Vector3.Distance(
                transform.position,
                zonaCorrectaF4.position
            );

        return distancia <= distanciaSnapF4;
    }

    public void ColocarF4()
    {
        if (zonaCorrectaF4 == null)
            return;

        transform.position =
            new Vector3(
                zonaCorrectaF4.position.x,
                transform.position.y,
                zonaCorrectaF4.position.z
            ) + offsetCentroF4;

        yaColocadoF4 = true;

        if (gameManager != null)
        {
            gameManager.RegistrarColocacion();
        }

        if (onPlacedF4 != null)
        {
            onPlacedF4.Invoke();
        }

        Debug.Log("¡Colocación correcta F4!");
    }
}