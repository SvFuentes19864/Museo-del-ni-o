using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class Fase2IntroManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narrador;

    [Header("Fade")]
    public Image fadeImage;
    public float duracionFadeIn = 1.5f;

    [Header("Iglesia")]
    public GameObject iglesia;

    [Header("Ruleta")]
    public SelectorArco selectorPiezas;

    [Header("Cámaras")]
    public CinemachineCamera cmIntro;
    public CinemachineCamera cmPrincipal;

    IEnumerator Start()
    {
        ActivarCamara(cmIntro);

        // Ocultar iglesia
        if (iglesia != null)
        {
            iglesia.SetActive(false);
        }

        // Empezar completamente blanco
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }

        // Iniciar narración inmediatamente
        if (narrador != null)
        {
            narrador.Play();
        }

        // Fade In en paralelo
        if (fadeImage != null)
        {
            StartCoroutine(
                FadeIn()
            );
        }

        // Cambio de cámara casi inmediato
        yield return new WaitForSeconds(0.1f);

        ActivarCamara(cmPrincipal);

        // Esperar a que termine el fade
        yield return new WaitForSeconds(
            duracionFadeIn
        );

        // Esperar a que termine la narración
        if (narrador != null)
        {
            yield return new WaitForSeconds(
                narrador.clip.length -
                duracionFadeIn
            );
        }

        // Mostrar iglesia
        if (iglesia != null)
        {
            iglesia.SetActive(true);
        }

        // Iniciar ruleta
        if (selectorPiezas != null)
        {
            selectorPiezas.IniciarRuleta();
        }
    }

    IEnumerator FadeIn()
    {
        Color color = fadeImage.color;

        float tiempo = 0f;

        while (tiempo < duracionFadeIn)
        {
            tiempo += Time.deltaTime;

            color.a =
                Mathf.Lerp(
                    1f,
                    0f,
                    tiempo / duracionFadeIn
                );

            fadeImage.color = color;

            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }

    void ActivarCamara(
        CinemachineCamera camaraActiva
    )
    {
        if (cmIntro != null)
        {
            cmIntro.Priority = 0;
        }

        if (cmPrincipal != null)
        {
            cmPrincipal.Priority = 0;
        }

        if (camaraActiva != null)
        {
            camaraActiva.Priority = 100;
        }
    }
}