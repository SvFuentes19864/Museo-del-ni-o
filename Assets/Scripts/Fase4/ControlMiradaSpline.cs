using UnityEngine;
using Unity.Cinemachine;

public class ControlMiradaSpline : MonoBehaviour
{
    [System.Serializable]
    public class PuntoMirada
    {
        [Range(0f, 1f)]
        public float posicionSpline;

        public Transform objetivo;
    }

    [Header("Referencias")]
    public CinemachineCamera camaraCart;

    [Header("Puntos de Mirada")]
    public PuntoMirada[] puntosMirada;

    private Transform objetivoActual;

    void Update()
    {
        if (camaraCart == null)
            return;

        CinemachineSplineDolly dolly =
            camaraCart.GetComponent<CinemachineSplineDolly>();

        if (dolly == null)
            return;

        float posicionActual = dolly.CameraPosition;

        Transform nuevoObjetivo = null;

        for (int i = 0; i < puntosMirada.Length; i++)
        {
            if (posicionActual >= puntosMirada[i].posicionSpline)
            {
                nuevoObjetivo = puntosMirada[i].objetivo;
            }
        }

        if (nuevoObjetivo != null &&
            nuevoObjetivo != objetivoActual)
        {
            objetivoActual = nuevoObjetivo;

            camaraCart.Target.TrackingTarget =
                objetivoActual;
        }
    }
}