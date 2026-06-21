using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class Fase1IntroManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narrador;

    [Header("Fade")]
    public Image fadeImage;
    public float duracionFadeIn = 1.5f;

    [Header("Ruleta")]
    public SelectorArco selectorArco;

    [Header("Cámaras")]
    public CinemachineCamera cmIntro;
    public CinemachineCamera cmPrincipal;

    private MeshRenderer[] renderersArco;

    IEnumerator Start()
    {
        ActivarCamara(cmIntro);

        // Ocultar pirámide ANTES del fade
        if (selectorArco != null)
        {
            renderersArco =
                selectorArco.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer r in renderersArco)
            {
                r.enabled = false;
            }
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

        yield return new WaitForSeconds(0.1f);

        ActivarCamara(cmPrincipal);

        // Esperar a que termine el fade
        yield return new WaitForSeconds(
            duracionFadeIn
        );

        // Esperar un poco más y mostrar pirámide
        yield return new WaitForSeconds(1f);

        // Esperar a que termine la narración
        if (narrador != null)
        {
            yield return new WaitForSeconds(
                narrador.clip.length - 1.5f
            );
        }

        if (renderersArco != null)
        {
            foreach (MeshRenderer r in renderersArco)
            {
                r.enabled = true;
            }
        }

        // Iniciar ruleta
        if (selectorArco != null)
        {
            selectorArco.IniciarRuleta();
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