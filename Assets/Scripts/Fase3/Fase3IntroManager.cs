using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    private MeshRenderer[] renderersPieza;

    IEnumerator Start()
    {
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

        // Fade In
        if (fadeImage != null)
        {
            yield return StartCoroutine(
                FadeIn()
            );
        }

        // Narración
        if (narradorFase3 != null)
        {
            narradorFase3.Play();

            yield return new WaitForSeconds(
                narradorFase3.clip.length
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
}