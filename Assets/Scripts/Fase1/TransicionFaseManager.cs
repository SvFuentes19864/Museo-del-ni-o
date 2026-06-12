using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransicionFaseManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narradorSalida;

    [Header("Tiempo para iniciar vuelo")]
    public float tiempoAntesDeVuelo = 18f;

    [Header("Cámaras")]
    public Camera camaraPrincipal;
    public Camera camaraCinematica;

    [Header("Puntos")]
    public Transform puntoInicioTransicion;
    public Transform puntoVuelo1;
    public Transform puntoVuelo2;
    public Transform puntoFade;

    [Header("Fade")]
    public Image fadeImage;

    [Header("Siguiente Escena")]
    public string siguienteEscena = "Fase2";

    public void IniciarTransicion()
    {
        StartCoroutine(SecuenciaTransicion());
    }

    IEnumerator SecuenciaTransicion()
    {
        // Iniciar audio inmediatamente
        if (narradorSalida != null)
        {
            narradorSalida.Play();
        }

        // Esperar hasta la parte donde menciona la máquina del tiempo
        yield return new WaitForSeconds(tiempoAntesDeVuelo);

        // Cambiar a cámara cinematográfica
        camaraPrincipal.enabled = false;
        camaraCinematica.enabled = true;

        camaraCinematica.transform.position =
            puntoInicioTransicion.position;

        camaraCinematica.transform.rotation =
            puntoInicioTransicion.rotation;

        // MOVIMIENTO 1
        yield return StartCoroutine(
            MoverCamara(
                puntoVuelo1,
                2f
            )
        );

        // MOVIMIENTO 2
        yield return StartCoroutine(
            MoverCamara(
                puntoVuelo2,
                2f
            )
        );

        // MOVIMIENTO FINAL
        yield return StartCoroutine(
            MoverCamara(
                puntoFade,
                1.5f
            )
        );

        // FADE
        yield return StartCoroutine(
            FadeBlanco()
        );

        // CAMBIO DE ESCENA
        SceneManager.LoadScene(
            siguienteEscena
        );
    }

    IEnumerator MoverCamara(
        Transform destino,
        float duracion
    )
    {
        Vector3 posicionInicial =
            camaraCinematica.transform.position;

        Quaternion rotacionInicial =
            camaraCinematica.transform.rotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            camaraCinematica.transform.position =
                Vector3.Lerp(
                    posicionInicial,
                    destino.position,
                    tiempo / duracion
                );

            camaraCinematica.transform.rotation =
                Quaternion.Lerp(
                    rotacionInicial,
                    destino.rotation,
                    tiempo / duracion
                );

            yield return null;
        }

        camaraCinematica.transform.position =
            destino.position;

        camaraCinematica.transform.rotation =
            destino.rotation;
    }

    IEnumerator FadeBlanco()
    {
        Color color = fadeImage.color;

        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime;

            color.a =
                Mathf.Lerp(
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
}