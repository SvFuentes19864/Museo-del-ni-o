using UnityEngine;
using System.Collections;

public class TormentaManager : MonoBehaviour
{
    [Header("Luz")]
    public Light directionalLight;

    public float intensidadFinal = 0.2f;
    public float duracionOscurecer = 10f;

    [Header("Audio")]
    public AudioSource audioBosque;
    public AudioSource audioTormenta;

    [Tooltip("Volumen final de la tormenta")]
    [Range(0f, 1f)]
    public float volumenFinalTormenta = 1f;

    [Header("Lluvia")]
    public ParticleSystem lluvia;

    [Header("Inundación")]
    public InundacionManager inundacionManager;

    public float retrasoInundacion = 8f;

    public void IniciarTormenta()
    {
        StartCoroutine(Tormenta());
    }

    IEnumerator Tormenta()
    {
        float intensidadInicial =
            directionalLight.intensity;

        float volumenBosqueInicial =
            audioBosque.volume;

        // Iniciar tormenta
        audioTormenta.volume = 0f;
        audioTormenta.Play();

        // Iniciar lluvia
        if (lluvia != null)
        {
            lluvia.Play();
        }

        // Programar inundación
        StartCoroutine(
            EsperarInundacion()
        );

        float tiempo = 0f;

        while (tiempo < duracionOscurecer)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionOscurecer;

            directionalLight.intensity =
                Mathf.Lerp(
                    intensidadInicial,
                    intensidadFinal,
                    t
                );

            audioBosque.volume =
                Mathf.Lerp(
                    volumenBosqueInicial,
                    0f,
                    t
                );

            audioTormenta.volume =
                Mathf.Lerp(
                    0f,
                    volumenFinalTormenta,
                    t
                );

            yield return null;
        }

        directionalLight.intensity =
            intensidadFinal;

        audioBosque.volume = 0f;
        audioTormenta.volume =
            volumenFinalTormenta;
    }

    IEnumerator EsperarInundacion()
    {
        yield return new WaitForSeconds(
            retrasoInundacion
        );

        if (inundacionManager != null)
        {
            inundacionManager.IniciarInundacion();
        }
    }
}