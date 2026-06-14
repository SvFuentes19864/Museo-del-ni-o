using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    IEnumerator Start()
    {
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

        // Fade In
        if (fadeImage != null)
        {
            yield return StartCoroutine(
                FadeIn()
            );
        }

        // Narración
        if (narrador != null)
        {
            narrador.Play();

            yield return new WaitForSeconds(
                narrador.clip.length
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
}