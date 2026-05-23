using UnityEngine;
using System.Collections;

public class SpawnPiramides : MonoBehaviour
{
    public GameObject[] piramides;
    private Vector3[] escalasOriginales;

    public GameObject[] ranchitos;
    private Vector3[] escalasRanchitos;

    public AudioSource audioSource;
    public AudioSource audioRanchitos;

    public GameObject personaje;

    void Start()
    {
        // PIRÁMIDES
        escalasOriginales = new Vector3[piramides.Length];

        for (int i = 0; i < piramides.Length; i++)
        {
            escalasOriginales[i] = piramides[i].transform.localScale;
            piramides[i].SetActive(false);
        }

        // RANCHITOS
        escalasRanchitos = new Vector3[ranchitos.Length];

        for (int i = 0; i < ranchitos.Length; i++)
        {
            escalasRanchitos[i] = ranchitos[i].transform.localScale;
            ranchitos[i].SetActive(false);
        }
    }

    public void ActivarPiramides()
    {
        StartCoroutine(AparecerUnaPorUna());
    }

    IEnumerator AparecerUnaPorUna()
    {
        // 🔥 reproducir audio UNA sola vez
        audioSource.PlayOneShot(audioSource.clip);

        float tiempoPorObjeto = audioSource.clip.length / piramides.Length;

        // primero pirámides
        for (int i = 0; i < piramides.Length; i++)
        {
            GameObject p = piramides[i];

            p.SetActive(true);
            audioSource.PlayOneShot(audioSource.clip);
            p.transform.localScale = Vector3.zero;

            yield return StartCoroutine(AnimarEscala(p, escalasOriginales[i], tiempoPorObjeto));
        }

        personaje.SetActive(true);

        // luego ranchitos
        float tiempoPorRanchito = audioSource.clip.length / ranchitos.Length;

        // luego ranchitos
        for (int i = 0; i < ranchitos.Length; i++)
        {
            GameObject r = ranchitos[i];

            r.SetActive(true);
            audioRanchitos.PlayOneShot(audioRanchitos.clip); // 🔥 sonido aquí
            r.transform.localScale = Vector3.zero;

            yield return StartCoroutine(AnimarEscala(r, escalasRanchitos[i], tiempoPorRanchito));
        }
    }

    IEnumerator AnimarEscala(GameObject obj, Vector3 escalaFinal, float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            obj.transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, t);
            yield return null;
        }

        obj.transform.localScale = escalaFinal;
    }
}