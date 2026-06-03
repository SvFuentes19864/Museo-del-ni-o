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

    [Header("Transportes")]
    public GameObject[] transportes;
    private Vector3[] escalasTransportes;
    public AudioSource audioTransportes;

    [Header("Caballos")]
    public GameObject[] caballos;
    private Vector3[] escalasCaballos;
    public AudioSource audioCaballos;

    [Header("NPCs")]
    public GameObject[] personajes;

    [Header("Farolillos")]
    public GameObject[] farolillos;
    private Vector3[] escalasFarolillos;

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

        // TRANSPORTES
        escalasTransportes = new Vector3[transportes.Length];

        for (int i = 0; i < transportes.Length; i++)
        {
            escalasTransportes[i] = transportes[i].transform.localScale;
            transportes[i].SetActive(false);
        }

        // CABALLOS
        escalasCaballos = new Vector3[caballos.Length];

        for (int i = 0; i < caballos.Length; i++)
        {
            escalasCaballos[i] = caballos[i].transform.localScale;
            caballos[i].SetActive(false);
        }

        // FAROLILLOS
        escalasFarolillos = new Vector3[farolillos.Length];

        for (int i = 0; i < farolillos.Length; i++)
        {
            escalasFarolillos[i] =
                farolillos[i].transform.localScale;

            farolillos[i].SetActive(false);
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
        // CAMINO
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

            yield return new WaitForSeconds(
                tiempoPorCasita
            );
        }

        // TRANSPORTES
        float tiempoPorTransporte = 0.4f;

        if (
            transportes.Length > 0 &&
            audioTransportes != null
        )
        {
            audioTransportes.PlayOneShot(
                audioTransportes.clip
            );
        }

        for (int i = 0; i < transportes.Length; i++)
        {
            GameObject t = transportes[i];

            t.SetActive(true);
            t.transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    t,
                    escalasTransportes[i],
                    tiempoPorTransporte
                )
            );
        }

        // CABALLOS
        float tiempoPorCaballo = 0.4f;

        if (
            caballos.Length > 0 &&
            audioCaballos != null
        )
        {
            audioCaballos.PlayOneShot(
                audioCaballos.clip
            );
        }

        for (int i = 0; i < caballos.Length; i++)
        {
            GameObject c = caballos[i];

            c.SetActive(true);
            c.transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    c,
                    escalasCaballos[i],
                    tiempoPorCaballo
                )
            );
        }

        // NPCs
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(true);
        }

        // FAROLILLOS
        float tiempoPorFarolillo = 0.15f;

        for (int i = 0; i < farolillos.Length; i++)
        {
            GameObject f = farolillos[i];

            f.SetActive(true);
            f.transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    f,
                    escalasFarolillos[i],
                    tiempoPorFarolillo
                )
            );
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
                Mathf.Lerp(
                    0.01f,
                    escalaFinal.x,
                    t
                ),
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