using UnityEngine;

public class DragObjectCerrito : MonoBehaviour
{
    public GameManager gameManager;

    public Transform zonaCorrecta;

    public GameObject outlineObject;

    private bool yaColocado = false;
    private bool isDragging = false;

    private Vector3 offsetCentro;

    void Start()
    {
        Renderer rendererPrincipal =
            GetComponentInChildren<Renderer>();

        if (rendererPrincipal == null)
        {
            Debug.LogWarning(
                "No se encontró Renderer en Cerrito."
            );

            return;
        }

        Bounds bounds =
            rendererPrincipal.bounds;

        offsetCentro = new Vector3(
            transform.position.x - bounds.center.x,
            0,
            transform.position.z - bounds.center.z
        );

        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (yaColocado)
        {
            return;
        }

        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (yaColocado)
        {
            return;
        }

        if (zonaCorrecta != null)
        {
            float distancia =
                Vector3.Distance(
                    transform.position,
                    zonaCorrecta.position
                );

            if (distancia < 3f)
            {
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

                SpawnF4_1 spawn =
                    FindObjectOfType<SpawnF4_1>();

                if (spawn != null)
                {
                    spawn.ActivarFase4();
                }

                Debug.Log(
                    "¡Cerrito del Carmen colocado correctamente!"
                );

                Debug.Log(
                    "¡Cerrito del Carmen colocado correctamente!"
                );
            }
        }
    }

    void Update()
    {
        if (isDragging && !yaColocado)
        {
            Ray ray =
                Camera.main.ScreenPointToRay(
                    Input.mousePosition
                );

            Plane plane =
                new Plane(
                    Vector3.up,
                    transform.position
                );

            float enter;

            if (plane.Raycast(ray, out enter))
            {
                Vector3 point =
                    ray.GetPoint(enter);

                transform.position =
                    new Vector3(
                        point.x,
                        transform.position.y,
                        point.z
                    );
            }
        }
    }

    void OnMouseEnter()
    {
        if (!yaColocado && outlineObject != null)
        {
            outlineObject.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }
}