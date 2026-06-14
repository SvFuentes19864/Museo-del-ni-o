using UnityEngine;
using System.Collections;

public class Fase2NarracionesManager : MonoBehaviour
{
    [Header("Narración Intermedia")]
    public AudioSource narracionIntermedia;
    public float tiempoAntesIntermedia = 2f;

    [Header("Narración Final")]
    public AudioSource narracionFinal;
    public float tiempoAntesFinal = 5f;

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
        if (narracionIntermedia != null)
        {
            narracionIntermedia.Play();

            yield return new WaitForSeconds(
                narracionIntermedia.clip.length
            );
        }

        // Esperar antes de la narración final
        yield return new WaitForSeconds(
            tiempoAntesFinal
        );

        // Reproducir narración final
        if (narracionFinal != null)
        {
            narracionFinal.Play();
        }
    }
}