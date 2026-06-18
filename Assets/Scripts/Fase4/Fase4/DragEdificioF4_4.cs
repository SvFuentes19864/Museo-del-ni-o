using UnityEngine;

public class DragEdificioF4_4 : MonoBehaviour
{
    public GameManager gameManager;

    public SpawnF4_4 spawnF4_4;

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
                "No se encontró Renderer en Edificio F4_4."
            );
            return;
        }

        Bounds bounds = rendererPrincipal.bounds;

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

                if (spawnF4_4 != null)
                {
                    spawnF4_4.ActivarParte4();
                }

                Debug.Log(
                    "¡Edificio F4_4 colocado correctamente!"
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