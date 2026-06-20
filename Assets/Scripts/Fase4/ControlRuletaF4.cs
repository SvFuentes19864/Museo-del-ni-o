using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class ControlRuletaF4 : MonoBehaviour
{
    [Header("Puntos de Ruleta")]
    public Transform[] puntosRuleta;

    [Header("Objeto Actual")]
    public GameObject objetoActual;

    [Header("Destino")]
    public Transform puntoInicioDrag;

    [Header("Cámara")]
    public CinemachineCamera camaraDestino;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Tecla R detectada");
            IniciarRuleta();
        }
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
    }

    IEnumerator Ruleta()
    {
        Debug.Log("Coroutine Ruleta iniciada");

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

            Debug.Log(
                "Saltando a: " +
                puntosRuleta[indice].name
            );

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
            camaraDestino.Priority = 100;
        }

        if (puntoInicioDrag != null)
        {
            yield return StartCoroutine(
                MoverSuavemente(
                    puntoInicioDrag.position
                )
            );
        }
    }

    IEnumerator MoverSuavemente(
        Vector3 destino
    )
    {
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
    }
}