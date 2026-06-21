using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

public class TransicionFaseManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narradorSalida;

    [Header("Sonido Máquina del Tiempo")]
    public AudioSource audioMaquinaTiempo;

    [Header("Tiempo para iniciar cinemática")]
    public float tiempoAntesDeVuelo = 18f;

    [Header("Cámaras")]
    public GameObject dollyCart;

    public CinemachineCamera cmPrincipal;
    public CinemachineCamera cmDolly;

    [Header("Duración de la cinemática")]
    public float duracionCinematica = 5f;

    [Header("Fade")]
    public Image fadeImage;

    [Tooltip("Cuántos segundos antes de terminar la cinemática comienza el fade")]
    public float tiempoAntesDelFade = 1.5f;

    [Header("Siguiente Escena")]
    public string siguienteEscena = "Fase2";

    public void IniciarTransicion()
    {
        StartCoroutine(SecuenciaTransicion());
    }

    IEnumerator SecuenciaTransicion()
    {
        // Asegurar que la cinemática esté apagada al inicio
        if (dollyCart != null)
        {
            dollyCart.SetActive(false);
        }

        // Audio narrador
        if (narradorSalida != null)
        {
            narradorSalida.Play();
        }

        // Esperar al segundo 18
        yield return new WaitForSeconds(tiempoAntesDeVuelo);

        // Iniciar cinemática
        if (dollyCart != null)
        {
            dollyCart.SetActive(true);
        }

        ActivarCamara(cmDolly);

        // Esperar hasta el momento de iniciar fade
        yield return new WaitForSeconds(
            Mathf.Max(
                0,
                duracionCinematica - tiempoAntesDelFade
            )
        );

        // Sonido de máquina del tiempo
        if (audioMaquinaTiempo != null)
        {
            audioMaquinaTiempo.Play();
        }

        // Fade mientras la cámara sigue moviéndose
        StartCoroutine(FadeBlanco());

        // Esperar el resto de la cinemática
        yield return new WaitForSeconds(tiempoAntesDelFade);

        // Cargar siguiente escena
        SceneManager.LoadScene(siguienteEscena);
    }

    IEnumerator FadeBlanco()
    {
        Color color = fadeImage.color;

        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime;

            color.a = Mathf.Lerp(
                0f,
                1f,
                tiempo
            );

            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    void ActivarCamara(
        CinemachineCamera camaraActiva
    )
    {
        if (cmPrincipal != null)
        {
            cmPrincipal.Priority = 0;
        }

        if (cmDolly != null)
        {
            cmDolly.Priority = 0;
        }

        if (camaraActiva != null)
        {
            camaraActiva.Priority = 100;
        }
    }
}