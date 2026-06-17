using UnityEngine;
using System.Collections;

public class SpawnF4_1 : MonoBehaviour
{
    [Header("Edificios Principales")]
    public GameObject[] edificiosPrincipales;
    private Vector3[] escalasEdificiosPrincipales;

    [Header("Cuadras Secundarias")]
    public GameObject[] cuadrasSecundarias;
    private Vector3[] escalasCuadrasSecundarias;

    [Header("Árboles")]
    public GameObject[] arbolesCiudad;
    private Vector3[] escalasArboles;

    [Header("NPCs")]
    public GameObject[] npcsCiudad;
    private Vector3[] escalasNPCs;

    [Header("Vehículos")]
    public GameObject[] vehiculosCiudad;
    private Vector3[] escalasVehiculos;

    [Header("Objetos Urbanos")]
    public GameObject[] objetosUrbanos;
    private Vector3[] escalasObjetosUrbanos;

    [Header("Audio")]
    public AudioSource audioCiudad;
    public AudioSource audioEdificios;
    public AudioSource audioCuadras;
    public AudioSource audioArboles;
    public AudioSource audioNPCs;
    public AudioSource audioVehiculos;
    public AudioSource audioObjetosUrbanos;

    [Header("Velocidades de Spawn")]
    public float tiempoSpawnEdificios = 0.5f;
    public float tiempoSpawnCuadras = 0.2f;
    public float tiempoSpawnArboles = 0.1f;
    public float tiempoSpawnNPCs = 0.1f;
    public float tiempoSpawnVehiculos = 0.1f;
    public float tiempoSpawnObjetosUrbanos = 0.1f;

    void Start()
    {
        // EDIFICIOS PRINCIPALES
        escalasEdificiosPrincipales =
            new Vector3[edificiosPrincipales.Length];

        for (int i = 0; i < edificiosPrincipales.Length; i++)
        {
            escalasEdificiosPrincipales[i] =
                edificiosPrincipales[i].transform.localScale;

            edificiosPrincipales[i].SetActive(false);
        }

        // CUADRAS SECUNDARIAS
        escalasCuadrasSecundarias =
            new Vector3[cuadrasSecundarias.Length];

        for (int i = 0; i < cuadrasSecundarias.Length; i++)
        {
            escalasCuadrasSecundarias[i] =
                cuadrasSecundarias[i].transform.localScale;

            cuadrasSecundarias[i].SetActive(false);
        }

        // ÁRBOLES
        escalasArboles =
            new Vector3[arbolesCiudad.Length];

        for (int i = 0; i < arbolesCiudad.Length; i++)
        {
            escalasArboles[i] =
                arbolesCiudad[i].transform.localScale;

            arbolesCiudad[i].SetActive(false);
        }

        // NPCs
        escalasNPCs =
            new Vector3[npcsCiudad.Length];

        for (int i = 0; i < npcsCiudad.Length; i++)
        {
            escalasNPCs[i] =
                npcsCiudad[i].transform.localScale;

            npcsCiudad[i].SetActive(false);
        }

        // VEHÍCULOS
        escalasVehiculos =
            new Vector3[vehiculosCiudad.Length];

        for (int i = 0; i < vehiculosCiudad.Length; i++)
        {
            escalasVehiculos[i] =
                vehiculosCiudad[i].transform.localScale;

            vehiculosCiudad[i].SetActive(false);
        }

        // OBJETOS URBANOS
        escalasObjetosUrbanos =
            new Vector3[objetosUrbanos.Length];

        for (int i = 0; i < objetosUrbanos.Length; i++)
        {
            escalasObjetosUrbanos[i] =
                objetosUrbanos[i].transform.localScale;

            objetosUrbanos[i].SetActive(false);
        }
    }

    public void ActivarFase4()
    {
        StartCoroutine(
            AparecerFase4()
        );
    }

    IEnumerator AparecerFase4()
    {
        // AUDIO CIUDAD
        if (audioCiudad != null)
        {
            audioCiudad.Play();
        }

        // EDIFICIOS PRINCIPALES
        for (int i = 0; i < edificiosPrincipales.Length; i++)
        {
            if (audioEdificios != null)
            {
                audioEdificios.PlayOneShot(
                    audioEdificios.clip
                );
            }

            edificiosPrincipales[i].SetActive(true);
            edificiosPrincipales[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    edificiosPrincipales[i],
                    escalasEdificiosPrincipales[i],
                    tiempoSpawnEdificios
                )
            );
        }

        // CUADRAS SECUNDARIAS
        for (int i = 0; i < cuadrasSecundarias.Length; i++)
        {
            if (audioCuadras != null)
            {
                audioCuadras.PlayOneShot(
                    audioCuadras.clip
                );
            }

            cuadrasSecundarias[i].SetActive(true);
            cuadrasSecundarias[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    cuadrasSecundarias[i],
                    escalasCuadrasSecundarias[i],
                    tiempoSpawnCuadras
                )
            );
        }

        // ÁRBOLES
        for (int i = 0; i < arbolesCiudad.Length; i++)
        {
            if (audioArboles != null)
            {
                audioArboles.PlayOneShot(
                    audioArboles.clip
                );
            }

            arbolesCiudad[i].SetActive(true);
            arbolesCiudad[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    arbolesCiudad[i],
                    escalasArboles[i],
                    tiempoSpawnArboles
                )
            );
        }

        // NPCs
        for (int i = 0; i < npcsCiudad.Length; i++)
        {
            if (audioNPCs != null)
            {
                audioNPCs.PlayOneShot(
                    audioNPCs.clip
                );
            }

            npcsCiudad[i].SetActive(true);
            npcsCiudad[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    npcsCiudad[i],
                    escalasNPCs[i],
                    tiempoSpawnNPCs
                )
            );
        }

        // VEHÍCULOS
        for (int i = 0; i < vehiculosCiudad.Length; i++)
        {
            if (audioVehiculos != null)
            {
                audioVehiculos.PlayOneShot(
                    audioVehiculos.clip
                );
            }

            vehiculosCiudad[i].SetActive(true);
            vehiculosCiudad[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    vehiculosCiudad[i],
                    escalasVehiculos[i],
                    tiempoSpawnVehiculos
                )
            );
        }

        // OBJETOS URBANOS
        for (int i = 0; i < objetosUrbanos.Length; i++)
        {
            if (audioObjetosUrbanos != null)
            {
                audioObjetosUrbanos.PlayOneShot(
                    audioObjetosUrbanos.clip
                );
            }

            objetosUrbanos[i].SetActive(true);
            objetosUrbanos[i].transform.localScale =
                Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    objetosUrbanos[i],
                    escalasObjetosUrbanos[i],
                    tiempoSpawnObjetosUrbanos
                )
            );
        }
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

        obj.transform.localScale =
            escalaFinal;
    }
}