using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camara;
    public AudioSource narracion;
    public Image fadeImage;

    [Header("Puntos Cinemáticos")]
    public Transform puntoInicial;
    public Transform puntoUno;
    public Transform puntoDos;
    public Transform puntoTres;
    public Transform puntoViajamos;

    [Header("Escena")]
    public string escenaFase1 = "Fase1";

    IEnumerator Start()
    {
        // Posición inicial
        camara.transform.position =
            puntoInicial.position;

        camara.transform.rotation =
            puntoInicial.rotation;

        yield return null;

        if (narracion != null)
        {
            narracion.Play();
        }

        // UNO
        yield return new WaitForSeconds(9f);

        yield return StartCoroutine(
            MoverCamara(
                puntoUno,
                0.3f
            )
        );

        // DOS
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(
            MoverCamara(
                puntoDos,
                0.3f
            )
        );

        // TRES
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(
            MoverCamara(
                puntoTres,
                0.3f
            )
        );

        // VIAJAMOS
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(
            MoverCamara(
                puntoViajamos,
                2f
            )
        );

        yield return StartCoroutine(
            FadeBlanco()
        );

        SceneManager.LoadScene(
            escenaFase1
        );
    }

    IEnumerator MoverCamara(
        Transform destino,
        float duracion
    )
    {
        Vector3 posicionInicial =
            camara.transform.position;

        Quaternion rotacionInicial =
            camara.transform.rotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            camara.transform.position =
                Vector3.Lerp(
                    posicionInicial,
                    destino.position,
                    tiempo / duracion
                );

            camara.transform.rotation =
                Quaternion.Lerp(
                    rotacionInicial,
                    destino.rotation,
                    tiempo / duracion
                );

            yield return null;
        }

        camara.transform.position =
            destino.position;

        camara.transform.rotation =
            destino.rotation;
    }

    IEnumerator FadeBlanco()
    {
        Color color =
            fadeImage.color;

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

            fadeImage.color =
                color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }
}