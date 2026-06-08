using UnityEngine;
using System.Collections;

public class SpawnFase3 : MonoBehaviour
{
    [Header("Parque")]
    public GameObject sueloParque;
    private Vector3 escalaSuelo;

    [Header("Árboles")]
    public GameObject[] arboles;
    private Vector3[] escalasArboles;

    [Header("Fuente")]
    public GameObject fuente;
    private Vector3 escalaFuente;

    [Header("Bancas")]
    public GameObject[] bancas;
    private Vector3[] escalasBancas;

    [Header("Edificios Emblemáticos")]
    public GameObject[] edificiosEmblematicos;
    private Vector3[] escalasEdificios;

    [Header("NPCs")]
    public GameObject[] personajes;

    [Header("Caravanas")]
    public GameObject[] caravanas;
    public float distanciaCaravana = 4f;
    public float duracionCaravana = 3f;

    [Header("Casas Alrededor")]
    public GameObject[] casasAlrededor;
    private Vector3[] escalasCasas;

    [Header("Audios")]
    public AudioSource audioParque;
    public AudioSource audioArboles;
    public AudioSource audioFuente;
    public AudioSource audioBancas;
    public AudioSource audioEdificios;
    public AudioSource audioCasas;

    void Start()
    {
        // SUELO
        if (sueloParque != null)
        {
            escalaSuelo = sueloParque.transform.localScale;
            sueloParque.SetActive(false);
        }

        // ÁRBOLES
        escalasArboles = new Vector3[arboles.Length];

        for (int i = 0; i < arboles.Length; i++)
        {
            escalasArboles[i] = arboles[i].transform.localScale;
            arboles[i].SetActive(false);
        }

        // FUENTE
        if (fuente != null)
        {
            escalaFuente = fuente.transform.localScale;
            fuente.SetActive(false);
        }

        // BANCAS
        escalasBancas = new Vector3[bancas.Length];

        for (int i = 0; i < bancas.Length; i++)
        {
            escalasBancas[i] = bancas[i].transform.localScale;
            bancas[i].SetActive(false);
        }

        // EDIFICIOS
        escalasEdificios = new Vector3[edificiosEmblematicos.Length];

        for (int i = 0; i < edificiosEmblematicos.Length; i++)
        {
            escalasEdificios[i] =
                edificiosEmblematicos[i].transform.localScale;

            edificiosEmblematicos[i].SetActive(false);
        }

        // NPCs
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(false);
        }

        // CARAVANAS
        for (int i = 0; i < caravanas.Length; i++)
        {
            caravanas[i].SetActive(false);
        }

        // CASAS
        escalasCasas = new Vector3[casasAlrededor.Length];

        for (int i = 0; i < casasAlrededor.Length; i++)
        {
            escalasCasas[i] =
                casasAlrededor[i].transform.localScale;

            casasAlrededor[i].SetActive(false);
        }
    }

    public void ActivarFase3()
    {
        StartCoroutine(AparecerFase3());
    }

    IEnumerator AparecerFase3()
    {
        // SUELO DEL PARQUE
        if (sueloParque != null)
        {
            if (audioParque != null)
            {
                audioParque.PlayOneShot(audioParque.clip);
            }

            sueloParque.SetActive(true);
            sueloParque.transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    sueloParque,
                    escalaSuelo,
                    1f
                )
            );
        }

        // ÁRBOLES
        for (int i = 0; i < arboles.Length; i++)
        {
            if (audioArboles != null)
            {
                audioArboles.PlayOneShot(audioArboles.clip);
            }

            arboles[i].SetActive(true);
            arboles[i].transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    arboles[i],
                    escalasArboles[i],
                    0.25f
                )
            );
        }

        // FUENTE
        if (fuente != null)
        {
            if (audioFuente != null)
            {
                audioFuente.PlayOneShot(audioFuente.clip);
            }

            fuente.SetActive(true);
            fuente.transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    fuente,
                    escalaFuente,
                    0.8f
                )
            );
        }

        // BANCAS
        for (int i = 0; i < bancas.Length; i++)
        {
            if (audioBancas != null)
            {
                audioBancas.PlayOneShot(audioBancas.clip);
            }

            bancas[i].SetActive(true);
            bancas[i].transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    bancas[i],
                    escalasBancas[i],
                    0.2f
                )
            );
        }

        // EDIFICIOS EMBLEMÁTICOS
        for (int i = 0; i < edificiosEmblematicos.Length; i++)
        {
            if (audioEdificios != null)
            {
                audioEdificios.PlayOneShot(audioEdificios.clip);
            }

            edificiosEmblematicos[i].SetActive(true);

            edificiosEmblematicos[i].transform.localScale =
                new Vector3(
                    escalasEdificios[i].x,
                    0.01f,
                    escalasEdificios[i].z
                );

            yield return StartCoroutine(
                AnimarConstruccion(
                    edificiosEmblematicos[i],
                    escalasEdificios[i],
                    1.2f
                )
            );
        }

        // NPCs
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(true);
        }

        // CARAVANAS
        for (int i = 0; i < caravanas.Length; i++)
        {
            caravanas[i].SetActive(true);

            StartCoroutine(
                MoverCaravana(
                    caravanas[i]
                )
            );
        }

        // CASAS ALREDEDOR
        for (int i = 0; i < casasAlrededor.Length; i++)
        {
            if (audioCasas != null)
            {
                audioCasas.PlayOneShot(audioCasas.clip);
            }

            casasAlrededor[i].SetActive(true);
            casasAlrededor[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    casasAlrededor[i],
                    escalasCasas[i],
                    0.2f
                )
            );
        }
    }

    IEnumerator MoverCaravana(
        GameObject caravana
    )
    {
        Vector3 posicionInicial =
            caravana.transform.position;

        Vector3 posicionFinal =
            posicionInicial +
            caravana.transform.forward *
            distanciaCaravana;

        float tiempo = 0f;

        while (tiempo < duracionCaravana)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionCaravana;

            caravana.transform.position =
                Vector3.Lerp(
                    posicionInicial,
                    posicionFinal,
                    t
                );

            yield return null;
        }

        caravana.transform.position =
            posicionFinal;
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

    IEnumerator AnimarConstruccion(
        GameObject obj,
        Vector3 escalaFinal,
        float duracion
    )
    {
        float tiempo = 0f;

        Vector3 posicionOriginal =
            obj.transform.position;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;

            float alturaActual =
                Mathf.Lerp(
                    0.01f,
                    escalaFinal.y,
                    t
                );

            obj.transform.localScale =
                new Vector3(
                    escalaFinal.x,
                    alturaActual,
                    escalaFinal.z
                );

            obj.transform.position =
                posicionOriginal -
                new Vector3(
                    0,
                    (escalaFinal.y - alturaActual) / 2f,
                    0
                );

            yield return null;
        }

        obj.transform.localScale =
            escalaFinal;

        obj.transform.position =
            posicionOriginal;
    }
}