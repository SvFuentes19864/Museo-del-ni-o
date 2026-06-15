using UnityEngine;
using System.Collections;

public class Fase3NarracionesManager : MonoBehaviour
{
    [Header("Narración Intermedia")]
    public AudioSource narracionIntermediaFase3;
    public float tiempoAntesIntermedia = 2f;

    [Header("Narración Final")]
    public AudioSource narracionFinalFase3;
    public float tiempoAntesFinal = 5f;

    [Header("Transición")]
    public TransicionFaseManager transicionFaseManager;

    [Tooltip("Segundos antes de terminar la narración final para iniciar la transición")]
    public float tiempoAntesTransicion = 3f;

    public void IniciarNarraciones()
    {
        StartCoroutine(
            SecuenciaNarraciones()
        );
    }

    IEnumerator SecuenciaNarraciones()
    {
        // Esperar antes de la narración intermedia
        yield return new WaitForSeconds(
            tiempoAntesIntermedia
        );

        // Reproducir narración intermedia
        if (narracionIntermediaFase3 != null)
        {
            narracionIntermediaFase3.Play();

            yield return new WaitForSeconds(
                narracionIntermediaFase3.clip.length
            );
        }

        // Esperar antes de la narración final
        yield return new WaitForSeconds(
            tiempoAntesFinal
        );

        // Reproducir narración final
        if (narracionFinalFase3 != null)
        {
            narracionFinalFase3.Play();

            float tiempoParaTransicion =
                Mathf.Max(
                    0f,
                    narracionFinalFase3.clip.length -
                    tiempoAntesTransicion
                );

            yield return new WaitForSeconds(
                tiempoParaTransicion
            );

            if (transicionFaseManager != null)
            {
                transicionFaseManager.IniciarTransicion();
            }
        }
    }
}