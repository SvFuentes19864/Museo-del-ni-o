using System.Collections.Generic;
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

    [Header("Reclamación por collider y tiempo")]
    [Tooltip("Segundos que la mano debe mantenerse dentro del BoxCollider antes de tomar control")]
    public float tiempoParaReclamar = 1f;

    [Header("Suavizado de movimiento")]
    [Tooltip("Igual que suavizadoAvatares en el tracker. Sube para más respuesta.")]
    public float suavizadoMovimiento = 10f;

    [Header("Zona correcta")]
    public Transform zonaCorrectaF4;
    public Vector3 offsetSnap = Vector3.zero;

    [Header("Objetos a ocultar al colocar")]
    public List<GameObject> objetosAOcultar = new();


    [Header("Evento al colocar")]
    public UnityEvent onPlacedF4;

    [Header("Última pieza")]
    public bool esUltimaPiezaF4 = false;

    [Header("Cursor F4")]
    public GameObject cursorF4;

    private bool desbloqueado = false;

    public void HabilitarArrastre()
    {
        desbloqueado = true;
        yaColocadoF4 = false;
    }

    private int handIdReclamante = -1;
    private float hoverTimer = 0f;

    private BoxCollider _col;

    void Start()
    {
        _col = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (!desbloqueado || yaColocadoF4 || handTracker == null) return;

        var positions = handTracker.handWorldPositions;

        if (handIdReclamante >= 0)
        {
            // mano desapareció (TCP "up")
            if (!positions.ContainsKey(handIdReclamante))
            {
                handTracker.manosReclamadas.Remove(handIdReclamante);
                handIdReclamante = -1;
                hoverTimer = 0f;
                return;
            }

            // seguir la mano, centrando el BoxCollider en la posición de la mano
            Vector3 pos = positions[handIdReclamante];
            Vector3 colOffset = _col != null
                ? transform.TransformPoint(_col.center) - transform.position
                : Vector3.zero;
            Vector3 destino = new Vector3(pos.x - colOffset.x, transform.position.y, pos.z - colOffset.z);
            float t = suavizadoMovimiento <= 0f ? 1f : 1f - Mathf.Exp(-suavizadoMovimiento * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, destino, t);

            // snap inmediato al entrar en la zona
            if (PuedeColocarseF4()) ColocarF4();
        }
        else
        {
            // buscar la mano más cercana cuya posición 3D esté dentro del BoxCollider
            var col = GetComponent<Collider>();
            if (col == null) return;

            var worldPositions = handTracker.handWorldPositions;
            float minDist = float.MaxValue;
            int nearestId = -1;

            foreach (var kvp in worldPositions)
            {
                if (handTracker.manosReclamadas.Contains(kvp.Key)) continue; // ya la tiene otro objeto
                // chequeo solo en XZ: el Y de la mano proyectada puede no coincidir con el collider
                Vector3 posXZ = new Vector3(kvp.Value.x, col.bounds.center.y, kvp.Value.z);
                if (!col.bounds.Contains(posXZ)) continue;
                float d = Vector3.Distance(transform.position, kvp.Value);
                if (d < minDist) { minDist = d; nearestId = kvp.Key; }
            }

            if (nearestId >= 0)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= tiempoParaReclamar)
                {
                    handIdReclamante = nearestId;
                    handTracker.manosReclamadas.Add(nearestId);
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
        if (zonaCorrectaF4 == null || !desbloqueado || _col == null) return false;
        var zonaCol = zonaCorrectaF4.GetComponent<Collider>();
        if (zonaCol == null) return false;
        // comparar solo en XZ para evitar fallos por diferencia de Y
        Bounds a = _col.bounds;
        Bounds b = zonaCol.bounds;
        return a.min.x <= b.max.x && a.max.x >= b.min.x &&
               a.min.z <= b.max.z && a.max.z >= b.min.z;
    }

    public void ColocarF4()
    {
        if (zonaCorrectaF4 == null) return;

        Vector3 colOffset = _col != null
            ? transform.TransformPoint(_col.center) - transform.position
            : Vector3.zero;
        transform.position = new Vector3(
            zonaCorrectaF4.position.x - colOffset.x,
            transform.position.y,
            zonaCorrectaF4.position.z - colOffset.z
        ) + offsetSnap;

        yaColocadoF4 = true;
        this.enabled = false;

        if (handIdReclamante >= 0)
        {
            handTracker.manosReclamadas.Remove(handIdReclamante);
            handIdReclamante = -1;
        }

        zonaCorrectaF4?.gameObject.SetActive(false); // oculta la zona una vez usada

        foreach (var obj in objetosAOcultar)
            if (obj != null) obj.SetActive(false);

        gameManager?.RegistrarColocacion();
        onPlacedF4?.Invoke();

        if (esUltimaPiezaF4 && cursorF4 != null)
            cursorF4.SetActive(false);

        Debug.Log($"[HandDraggableF4] ¡Colocación correcta! → {gameObject.name}");
    }

}
