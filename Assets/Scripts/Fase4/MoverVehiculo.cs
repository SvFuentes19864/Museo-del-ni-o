using UnityEngine;

public class MoverVehiculo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 3f;

    void Update()
    {
        transform.Translate(
            Vector3.forward *
            velocidad *
            Time.deltaTime
        );
    }
}