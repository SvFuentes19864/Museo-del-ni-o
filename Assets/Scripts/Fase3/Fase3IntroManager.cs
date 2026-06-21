using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class Fase3IntroManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narradorFase3;

    [Header("Fade")]
    public Image fadeImage;
    public float duracionFadeIn = 1.5f;

    [Header("Pieza Principal")]
    public GameObject piezaParque;

    [Header("Ruleta")]
    public SelectorArco selectorParque;

    [Header("Cámaras")]
    public CinemachineCamera cmIntro;
    public CinemachineCamera cmPrincipal;

    private MeshRenderer[] renderersPieza;

    IEnumerator Start()
    {
        ActivarCamara(cmIntro);

        // Ocultar visualmente la pieza
        if (piezaParque != null)
        {
            renderersPieza =
                piezaParque.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer r in renderersPieza)
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
        if (narradorFase3 != null)
        {
            narradorFase3.Play();
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
        if (narradorFase3 != null)
        {
            yield return new WaitForSeconds(
                narradorFase3.clip.length -
                duracionFadeIn
            );
        }

        // Mostrar visualmente la pieza
        if (renderersPieza != null)
        {
            foreach (MeshRenderer r in renderersPieza)
            {
                r.enabled = true;
            }
        }

        // Iniciar ruleta
        if (selectorParque != null)
        {
            selectorParque.IniciarRuleta();
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