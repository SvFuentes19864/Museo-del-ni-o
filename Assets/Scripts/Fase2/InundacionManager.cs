using UnityEngine;
using System.Collections;

public class InundacionManager : MonoBehaviour
{
    [Header("Agua Principal")]
    public GameObject agua;

    [Header("NPCs")]
    public GameObject[] npcs;

    [Header("Esfera Volcán")]
    public GameObject esferaVolcan;
    public float desplazamientoInicialEsfera = 2f;
    public float duracionEsfera = 2f;

    [Header("Agua Volcán")]
    public GameObject aguaVolcan;
    public float duracionAguaVolcan = 3f;

    [Header("Retrasos")]
    public float retrasoAguaVolcan = 0.5f;
    public float retrasoInundacionPrincipal = 0.5f;

    [Header("Inundación Principal")]
    public float alturaFinal = 8f;
    public float duracion = 20f;

    private Vector3 posicionFinalEsfera;
    private Vector3 posicionInicialEsfera;

    private Vector3 escalaFinalAguaVolcan;
    private Vector3 escalaInicialAguaVolcan;

    void Start()
    {
        if (agua != null)
        {
            agua.SetActive(false);
        }

        if (esferaVolcan != null)
        {
            posicionFinalEsfera =
                esferaVolcan.transform.position;

            posicionInicialEsfera =
                posicionFinalEsfera -
                new Vector3(
                    0f,
                    desplazamientoInicialEsfera,
                    0f
                );

            esferaVolcan.transform.position =
                posicionInicialEsfera;
        }

        if (aguaVolcan != null)
        {
            escalaFinalAguaVolcan =
                aguaVolcan.transform.localScale;

            escalaInicialAguaVolcan =
                new Vector3(
                    escalaFinalAguaVolcan.x,
                    0f,
                    escalaFinalAguaVolcan.z
                );

            aguaVolcan.transform.localScale =
                escalaInicialAguaVolcan;
        }
    }

    public void IniciarInundacion()
    {
        StartCoroutine(
            SecuenciaInundacion()
        );
    }

    IEnumerator SecuenciaInundacion()
    {
        // Esfera emerge
        if (esferaVolcan != null)
        {
            yield return StartCoroutine(
                EmergerEsfera()
            );
        }

        // Espera configurable
        yield return new WaitForSeconds(
            retrasoAguaVolcan
        );

        // Agua baja por el volcán
        if (aguaVolcan != null)
        {
            yield return StartCoroutine(
                DesplegarAguaVolcan()
            );
        }

        // Espera configurable
        yield return new WaitForSeconds(
            retrasoInundacionPrincipal
        );

        // Inundación principal
        if (agua != null)
        {
            agua.SetActive(true);
        }

        StartCoroutine(
            DesaparecerNPCsDespues()
        );

        StartCoroutine(
            SubirAgua()
        );
    }

    IEnumerator EmergerEsfera()
    {
        float tiempo = 0f;

        while (tiempo < duracionEsfera)
        {
            tiempo += Time.deltaTime;

            esferaVolcan.transform.position =
                Vector3.Lerp(
                    posicionInicialEsfera,
                    posicionFinalEsfera,
                    tiempo / duracionEsfera
                );

            yield return null;
        }

        esferaVolcan.transform.position =
            posicionFinalEsfera;
    }

    IEnumerator DesplegarAguaVolcan()
    {
        float tiempo = 0f;

        while (tiempo < duracionAguaVolcan)
        {
            tiempo += Time.deltaTime;

            aguaVolcan.transform.localScale =
                Vector3.Lerp(
                    escalaInicialAguaVolcan,
                    escalaFinalAguaVolcan,
                    tiempo / duracionAguaVolcan
                );

            yield return null;
        }

        aguaVolcan.transform.localScale =
            escalaFinalAguaVolcan;
    }

    IEnumerator DesaparecerNPCsDespues()
    {
        yield return new WaitForSeconds(5f);

        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i] != null)
            {
                npcs[i].SetActive(false);
            }
        }
    }

    IEnumerator SubirAgua()
    {
        Vector3 posicionInicial =
            agua.transform.position;

        Vector3 posicionFinal =
            new Vector3(
                posicionInicial.x,
                alturaFinal,
                posicionInicial.z
            );

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;

            agua.transform.position =
                Vector3.Lerp(
                    posicionInicial,
                    posicionFinal,
                    t
                );

            yield return null;
        }

        agua.transform.position =
            posicionFinal;
    }
}