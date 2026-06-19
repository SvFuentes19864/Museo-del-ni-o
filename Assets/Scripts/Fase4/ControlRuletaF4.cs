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

        Debug.Log(
            "Ganador: " +
            puntosRuleta[ganador].name
        );
    }
}