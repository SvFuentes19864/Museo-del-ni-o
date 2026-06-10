using UnityEngine;
using System.Collections;

public class SelectorArco : MonoBehaviour
{
    [Header("Posiciones")]
    public Transform[] posiciones;

    [Header("Configuración")]
    public float tiempoInicial = 0.05f;
    public float incrementoVelocidad = 0.03f;
    public int saltosMinimos = 20;
    public int saltosMaximos = 35;

    private int ultimoGanador = -1;

    void Start()
    {
        StartCoroutine(Ruleta());
    }

    IEnumerator Ruleta()
    {
        if (posiciones == null || posiciones.Length == 0)
        {
            Debug.LogWarning("No hay posiciones asignadas.");
            yield break;
        }

        int ganador;

        do
        {
            ganador = Random.Range(0, posiciones.Length);
        }
        while (
            posiciones.Length > 1 &&
            ganador == ultimoGanador
        );

        ultimoGanador = ganador;

        int indiceActual = 0;

        int totalSaltos =
            Random.Range(
                saltosMinimos,
                saltosMaximos + 1
            );

        float espera = tiempoInicial;

        for (int i = 0; i < totalSaltos; i++)
        {
            if (posiciones[indiceActual] != null)
            {
                transform.position =
                    posiciones[indiceActual].position;
            }

            indiceActual++;

            if (indiceActual >= posiciones.Length)
            {
                indiceActual = 0;
            }

            yield return new WaitForSeconds(espera);

            espera += incrementoVelocidad;
        }

        if (posiciones[ganador] != null)
        {
            transform.position =
                posiciones[ganador].position;
        }

        Debug.Log(
            "Jugador seleccionado: " +
            (ganador + 1)
        );
    }
}