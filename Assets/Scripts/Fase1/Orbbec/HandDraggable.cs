using UnityEngine;
using UnityEngine.Events;

public class HandDraggable : MonoBehaviour
{
    [Header("GameManager")]
    public GameManager gameManager;

    [Header("Colocación")]
    public bool yaColocado = false;

    [Header("Zona correcta")]
    public Transform zonaCorrecta;

    [Header("Distancia de snap")]
    public float distanciaSnap = 3f;

    [Header("Evento al colocar")]
    public UnityEvent onPlaced;

    private Vector3 offsetCentro;

    void Start()
    {
        Renderer rendererPrincipal =
            GetComponent<Renderer>();

        if (rendererPrincipal != null)
        {
            Bounds bounds =
                rendererPrincipal.bounds;

            offsetCentro = new Vector3(
                transform.position.x - bounds.center.x,
                0,
                transform.position.z - bounds.center.z
            );
        }
    }

    void Update()
    {
        if (yaColocado)
            return;

        if (PuedeColocarse())
        {
            Colocar();
        }
    }

    public bool PuedeColocarse()
    {
        if (zonaCorrecta == null)
            return false;

        float distancia =
            Vector3.Distance(
                transform.position,
                zonaCorrecta.position
            );

        return distancia <= distanciaSnap;
    }

    public void Colocar()
    {
        if (zonaCorrecta == null)
            return;

        transform.position =
            new Vector3(
                zonaCorrecta.position.x,
                transform.position.y,
                zonaCorrecta.position.z
            ) + offsetCentro;

        yaColocado = true;

        if (gameManager != null)
        {
            gameManager.RegistrarColocacion();
        }

        if (onPlaced != null)
        {
            onPlaced.Invoke();
        }

        GameObject orbbec =
            GameObject.Find("Orbbec");

        if (orbbec != null)
        {
            orbbec.SetActive(false);
        }

        Debug.Log("¡Colocación correcta!");
    }
}