using UnityEngine;

public class DragObject : MonoBehaviour
{

    public GameManager gameManager;
    private bool yaColocado = false;
    private bool isDragging = false;
    private float distance;
    private Color colorOriginal;
    
    public Transform zonaCorrecta;

    void Start()
    {
        colorOriginal = GetComponent<Renderer>().material.color;
    }

    void OnMouseDown()
    {
        // Si ya está colocado, no hacer nada
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
                distance = Vector3.Distance(transform.position, Camera.main.transform.position);
                isDragging = true;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (zonaCorrecta != null)
        {
            float distancia = Vector3.Distance(transform.position, zonaCorrecta.position);

            if (distancia < 3f)
            {
                Vector3 offset = transform.position - GetComponent<Renderer>().bounds.center;

                transform.position = new Vector3(
                    zonaCorrecta.position.x,
                    transform.position.y,
                    zonaCorrecta.position.z
                ) + offset;

                if (!yaColocado)
                {
                    yaColocado = true;
                    gameManager.RegistrarColocacion();

                    // 🔥 activar aparición de pirámides
                    FindObjectOfType<SpawnPiramides>().ActivarPiramides();
                }

                Debug.Log("¡Colocación correcta!");
            }
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 point = ray.GetPoint(distance);
            transform.position = new Vector3(point.x, transform.position.y, point.z);
        }
    }

    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = colorOriginal;
    }
}