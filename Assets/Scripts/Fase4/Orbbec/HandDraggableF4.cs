using UnityEngine;
using UnityEngine.Events;

public class HandDraggableF4 : MonoBehaviour
{
    [Header("Tracker")]
    public HandTrackerF4 handTracker;

    [Header("GameManager")]
    public GameManager gameManager;

    [Header("Colocación")]
    public bool yaColocadoF4 = true;

    [Header("Timer de desbloqueo")]
    [Tooltip("Segundos hasta que el objeto se puede manipular.")]
    public float tiempoDesbloqueo = 20f;

    [Header("Reclamación por posición y tiempo")]
    [Tooltip("Radio en espacio de pantalla (0-1). 0.1 = el orbe debe estar dentro del 10% del tamaño de pantalla desde el centro del objeto")]
    public float radioReclamacion = 0.1f;
    [Tooltip("Segundos que el orbe debe mantenerse dentro del radio antes de tomar control")]
    public float tiempoParaReclamar = 1f;

    [Header("Zona correcta")]
    public Transform zonaCorrectaF4;

    [Header("Distancia de snap")]
    public float distanciaSnapF4 = 3f;

    [Header("Evento al colocar")]
    public UnityEvent onPlacedF4;

    [Header("Última pieza")]
    public bool esUltimaPiezaF4 = false;

    [Header("Cursor F4")]
    public GameObject cursorF4;

    private float timerDesbloqueo = 0f;
    private bool desbloqueado = false;

    private int handIdReclamante = -1;  // ID de la mano que controla este objeto (-1 = libre)
    private float hoverTimer = 0f;       // tiempo acumulado dentro del radio
    private bool lastPressed = false;

    private Vector3 offsetCentroF4;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            Bounds bounds = r.bounds;
            offsetCentroF4 = new Vector3(
                transform.position.x - bounds.center.x,
                0,
                transform.position.z - bounds.center.z
            );
        }
    }

    void Update()
    {
        // fase 1: esperar desbloqueo por timer
        if (!desbloqueado)
        {
            timerDesbloqueo += Time.deltaTime;
            if (timerDesbloqueo >= tiempoDesbloqueo)
            {
                desbloqueado = true;
                yaColocadoF4 = false;
                Debug.Log($"[HandDraggableF4] {gameObject.name} desbloqueado.");
            }
            return;
        }

        if (yaColocadoF4 || handTracker == null) return;

        var positions = handTracker.handWorldPositions;

        if (handIdReclamante >= 0)
        {
            // la mano reclamante desapareció → liberar
            if (!positions.ContainsKey(handIdReclamante))
            {
                Debug.Log($"[HandDraggableF4] Mano {handIdReclamante} desapareció — {gameObject.name} liberado.");
                handIdReclamante = -1;
                hoverTimer = 0f;
                lastPressed = false;
                return;
            }

            // seguir la mano reclamante
            Vector3 pos = positions[handIdReclamante];
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);

            // detectar soltar (press → release) → intentar snap
            bool pressed = handTracker.handPressedStates.TryGetValue(handIdReclamante, out bool p) && p;
            if (lastPressed && !pressed && PuedeColocarseF4())
                ColocarF4();
            lastPressed = pressed;
        }
        else
        {
            // buscar la mano más cercana cuyo orbe esté sobre el objeto en pantalla
            if (Camera.main == null) return;

            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.z <= 0f) return; // objeto detrás de la cámara

            var viewports = handTracker.handViewportPositions;
            float minDist = float.MaxValue;
            int nearestId = -1;

            foreach (var kvp in viewports)
            {
                float d = Vector2.Distance(new Vector2(vp.x, vp.y), kvp.Value);
                if (d < radioReclamacion && d < minDist) { minDist = d; nearestId = kvp.Key; }
            }

            if (nearestId >= 0)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= tiempoParaReclamar)
                {
                    handIdReclamante = nearestId;
                    Debug.Log($"[HandDraggableF4] {gameObject.name} reclamado por mano {nearestId}");
                }
            }
            else
            {
                hoverTimer = 0f; // salió antes de completar el tiempo → resetear
            }
        }
    }

    public bool PuedeColocarseF4()
    {
        if (zonaCorrectaF4 == null || !desbloqueado) return false;
        return Vector3.Distance(transform.position, zonaCorrectaF4.position) <= distanciaSnapF4;
    }

    public void ColocarF4()
    {
        if (zonaCorrectaF4 == null) return;

        transform.position = new Vector3(
            zonaCorrectaF4.position.x,
            transform.position.y,
            zonaCorrectaF4.position.z
        ) + offsetCentroF4;

        yaColocadoF4 = true;
        this.enabled = false;

        gameManager?.RegistrarColocacion();
        onPlacedF4?.Invoke();

        if (esUltimaPiezaF4 && cursorF4 != null)
            cursorF4.SetActive(false);

        Debug.Log($"[HandDraggableF4] ¡Colocación correcta! → {gameObject.name}");
    }

    void OnDrawGizmos()
    {
        if (yaColocadoF4) return;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, radioReclamacion);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radioReclamacion);
    }
}
