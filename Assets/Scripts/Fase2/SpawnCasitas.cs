using UnityEngine;
using System.Collections;

public class SpawnCasitas : MonoBehaviour
{
    [Header("Camino")]
    public GameObject camino;
    public AudioSource audioCamino;

    private Vector3 escalaCamino;

    [Header("Casitas")]
    public GameObject[] casitas;
    private Vector3[] escalasCasitas;

    public AudioSource audioCasitas;

    [Header("NPCs")]
    public GameObject[] personajes;

    void Start()
    {
        // CAMINO
        if (camino != null)
        {
            escalaCamino = camino.transform.localScale;
            camino.SetActive(false);
        }

        // CASITAS
        escalasCasitas = new Vector3[casitas.Length];

        for (int i = 0; i < casitas.Length; i++)
        {
            escalasCasitas[i] = casitas[i].transform.localScale;
            casitas[i].SetActive(false);
        }

        // NPCs
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(false);
        }
    }

    public void ActivarCasitas()
    {
        StartCoroutine(AparecerCasitas());
    }

    IEnumerator AparecerCasitas()
    {
        // CAMINO PRIMERO
        if (camino != null)
        {
            camino.SetActive(true);

            camino.transform.localScale = new Vector3(
                0.01f,
                escalaCamino.y,
                escalaCamino.z
            );

            if (audioCamino != null)
            {
                audioCamino.Play();
            }

            yield return StartCoroutine(
                AnimarCamino(
                    camino,
                    escalaCamino,
                    4f
                )
            );

            // cortar el sonido al terminar la animación
            if (audioCamino != null)
            {
                audioCamino.Stop();
            }
        }

        // CASITAS
        float tiempoPorCasita = 0.3f;

        for (int i = 0; i < casitas.Length; i += 3)
            {
                if (audioCasitas != null)
                {
                    audioCasitas.PlayOneShot(audioCasitas.clip);
                }

                for (int j = i; j < i + 3 && j < casitas.Length; j++)
                {
                    GameObject c = casitas[j];

                    c.SetActive(true);
                    c.transform.localScale = Vector3.zero;

                    StartCoroutine(
                        AnimarEscala(
                            c,
                            escalasCasitas[j],
                            tiempoPorCasita
                        )
                    );
                }

                yield return new WaitForSeconds(tiempoPorCasita);
            }

        // NPCs AL FINAL
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(true);
        }
    }

    IEnumerator AnimarCamino(
        GameObject obj,
        Vector3 escalaFinal,
        float duracion
    )
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;

            obj.transform.localScale = new Vector3(
                Mathf.Lerp(0.01f, escalaFinal.x, t),
                escalaFinal.y,
                escalaFinal.z
            );

            yield return null;
        }

        obj.transform.localScale = escalaFinal;
    }

    IEnumerator AnimarEscala(
        GameObject obj,
        Vector3 escalaFinal,
        float duracion
    )
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;

            obj.transform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    escalaFinal,
                    t
                );

            yield return null;
        }

        obj.transform.localScale = escalaFinal;
    }
}