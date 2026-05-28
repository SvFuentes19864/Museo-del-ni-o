using UnityEngine;

public class DragObject : MonoBehaviour
{
    public GameManager gameManager;

    public Transform zonaCorrecta;

    public GameObject outlineObject;

    private bool yaColocado = false;
    private bool isDragging = false;

    private float distance;

    private Vector3 offsetCentro;

    void Start()
    {
        // guardar SOLO offset horizontal
        Bounds bounds = GetComponent<Renderer>().bounds;

        offsetCentro = new Vector3(
            transform.position.x - bounds.center.x,
            0,
            transform.position.z - bounds.center.z
        );

        // apagar outline al iniciar
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                distance = Vector3.Distance(
                    transform.position,
                    Camera.main.transform.position
                );

                isDragging = true;
            }
        }
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
            float distancia = Vector3.Distance(
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

                gameManager.RegistrarColocacion();

                FindObjectOfType<SpawnPiramides>()
                    .ActivarPiramides();

                Debug.Log("¡Colocación correcta!");
            }
        }
    }

    void Update()
    {
        if (isDragging && !yaColocado)
        {
            Ray ray =
                Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 point = ray.GetPoint(distance);

            transform.position = new Vector3(
                point.x,
                transform.position.y,
                point.z
            );
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