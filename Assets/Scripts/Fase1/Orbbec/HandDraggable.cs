using UnityEngine;

public class HandDraggable : MonoBehaviour
{
    [Header("Colocación")]
    public bool yaColocado = false;

    [Header("Zona correcta")]
    public Transform zonaCorrecta;

    [Header("Distancia de snap")]
    public float distanciaSnap = 3f;

    // NUEVO: El objeto ahora revisa su propia posición automáticamente
    void Update()
    {
        // Si ya se colocó en su lugar, ignoramos el resto del código
        if (yaColocado)
            return;

        // Si aún no está colocado, verificamos constantemente si está cerca
        if (PuedeColocarse())
        {
            Colocar(); // Se engancha solo
        }
    }

    public bool PuedeColocarse()
    {
        if (zonaCorrecta == null)
            return false;

        float distancia = Vector3.Distance(
            transform.position,
            zonaCorrecta.position
        );

        return distancia <= distanciaSnap;
    }

    public void Colocar()
    {
        if (zonaCorrecta == null)
            return;

        // Hace el snap matemático, manteniendo su propia altura en Y
        transform.position = new Vector3(
            zonaCorrecta.position.x,
            transform.position.y,
            zonaCorrecta.position.z
        );

        yaColocado = true;
    }
}