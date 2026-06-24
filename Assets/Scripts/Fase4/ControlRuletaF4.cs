using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class ControlRuletaF4 : MonoBehaviour
{
    [Header("Orbbec")]
    public OrbbecUnity.OrbbecDevice orbbecDevice;

    [Header("Puntos de Ruleta")]
    public Transform[] puntosRuleta;

    [Header("Objeto Actual")]
    public GameObject objetoActual;

    [Header("Destino")]
    public Transform puntoInicioDrag;

    [Header("Cámara")]
    public CinemachineCamera camaraDestino;

    [Header("Hand Tracking F4")]
    public HandTrackerF4 handTrackerF4;

    [Header("Ruleta")]
    public int saltosMinimos = 8;
    public int saltosMaximos = 12;

    public float tiempoEntreSaltos = 0.15f;

    [Header("Movimiento Final")]
    public float duracionMovimientoFinal = 1f;

    [Header("Audio")]
    public AudioSource audioPop;
    public AudioSource audioGanador;

    void Start()
    {
        Debug.Log("ControlRuletaF4 iniciado");
    }

    public void IniciarRuleta()
    {
        Debug.Log("Iniciando ruleta");
        StartCoroutine(Ruleta());
    }

    public void ConfigurarRuleta(
        GameObject nuevoObjeto,
        Transform nuevoInicioDrag,
        CinemachineCamera nuevaCamara
    )
    {
        objetoActual = nuevoObjeto;
        puntoInicioDrag = nuevoInicioDrag;
        camaraDestino = nuevaCamara;

        Debug.Log(
            "ConfigurarRuleta -> " +
            nuevoObjeto.name
        );
    }

    IEnumerator EncenderOrbbec()
    {
        yield return new WaitForSeconds(1f);

        if (orbbecDevice != null)
        {
            orbbecDevice.enabled = true;

            Debug.Log("ORBBEC F4 ACTIVADO");
        }
        else
        {
            Debug.LogError(
                "orbbecDevice es NULL"
            );
        }
    }

    IEnumerator Ruleta()
    {
        Debug.Log(
            "COROUTINE RULETA -> " +
            objetoActual.name
        );

        if (
            objetoActual == null ||
            puntosRuleta == null ||
            puntosRuleta.Length == 0
        )
        {
            Debug.LogError(
                "Faltan referencias en ControlRuletaF4"
            );

            yield break;
        }

        int ganador =
            Random.Range(
                0,
                puntosRuleta.Length
            );

        int totalSaltos =
            Random.Range(
                saltosMinimos,
                saltosMaximos + 1
            );

        for (int i = 0; i < totalSaltos; i++)
        {
            int indice =
                Random.Range(
                    0,
                    puntosRuleta.Length
                );

            objetoActual.transform.position =
                puntosRuleta[indice].position;

            if (audioPop != null)
            {
                audioPop.Play();
            }

            yield return new WaitForSeconds(
                tiempoEntreSaltos
            );
        }

        objetoActual.transform.position =
            puntosRuleta[ganador].position;

        if (audioGanador != null)
        {
            audioGanador.Play();
        }

        Debug.Log(
            "Ganador: " +
            puntosRuleta[ganador].name
        );

        yield return new WaitForSeconds(1f);

        if (camaraDestino != null)
        {
            Debug.Log(
                "Activando cámara: " +
                camaraDestino.name
            );

            camaraDestino.Priority = 100;
        }
        else
        {
            Debug.LogError(
                "camaraDestino es NULL"
            );
        }

        if (puntoInicioDrag != null)
        {
            Debug.Log(
                "VOY A MOVER A INICIO DRAG -> " +
                puntoInicioDrag.name
            );

            yield return StartCoroutine(
                MoverSuavemente(
                    puntoInicioDrag.position
                )
            );
        }
        else
        {
            Debug.LogError(
                "puntoInicioDrag es NULL"
            );
        }
    }

    IEnumerator MoverSuavemente(
        Vector3 destino
    )
    {
        Debug.Log(
            "ENTRE A MOVER SUAVEMENTE"
        );

        Vector3 posicionInicial =
            objetoActual.transform.position;

        float tiempo = 0f;

        while (
            tiempo < duracionMovimientoFinal
        )
        {
            tiempo += Time.deltaTime;

            objetoActual.transform.position =
                Vector3.Lerp(
                    posicionInicial,
                    destino,
                    tiempo /
                    duracionMovimientoFinal
                );

            yield return null;
        }

        objetoActual.transform.position =
            destino;

        Debug.Log(
            "TERMINE MOVER SUAVEMENTE"
        );

        if (
            handTrackerF4 != null &&
            objetoActual != null
        )
        {
            handTrackerF4.CambiarObjetoF4(
                objetoActual.transform
            );

            Debug.Log(
                "Objeto asignado al HandTrackerF4: " +
                objetoActual.name
            );
        }
        else
        {
            Debug.LogError(
                "handTrackerF4 o objetoActual son NULL"
            );
        }

        StartCoroutine(
            EncenderOrbbec()
        );
    }
}