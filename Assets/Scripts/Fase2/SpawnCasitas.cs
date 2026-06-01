using UnityEngine;
using System.Collections;

public class SpawnCasitas : MonoBehaviour
{
    public GameObject[] casitas;
    private Vector3[] escalasCasitas;

    public AudioSource audioCasitas;

    public GameObject[] personajes;

    void Start()
    {
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
        float tiempoPorCasita = 0.3f;

        for (int i = 0; i < casitas.Length; i++)
        {
            GameObject c = casitas[i];

            c.SetActive(true);
            audioCasitas.PlayOneShot(audioCasitas.clip);

            c.transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    c,
                    escalasCasitas[i],
                    tiempoPorCasita
                )
            );
        }

        // NPCs aparecen al final
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(true);
        }
    }

    IEnumerator AnimarEscala(GameObject obj, Vector3 escalaFinal, float duracion)
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