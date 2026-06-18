using UnityEngine;
using System.Collections;

public class SpawnF4_5_C : MonoBehaviour
{
    [Header("Edificios Principales")]
    public GameObject[] edificiosPrincipales;
    private Vector3[] escalasEdificios;

    [Header("Cuadras")]
    public GameObject[] cuadras;
    private Vector3[] escalasCuadras;

    [Header("NPCs")]
    public GameObject[] npcs;
    private Vector3[] escalasNPCs;

    [Header("Vehículos")]
    public GameObject[] vehiculos;
    private Vector3[] escalasVehiculos;

    [Header("Objetos Urbanos")]
    public GameObject[] objetosUrbanos;
    private Vector3[] escalasObjetos;

    [Header("Audio")]
    public AudioSource audioCiudad;
    public AudioSource audioEdificios;
    public AudioSource audioCuadras;
    public AudioSource audioNPCs;
    public AudioSource audioVehiculos;
    public AudioSource audioObjetosUrbanos;

    [Header("Velocidades")]
    public float tiempoSpawnEdificios = 0.5f;
    public float tiempoSpawnCuadras = 0.2f;
    public float tiempoSpawnNPCs = 0.1f;
    public float tiempoSpawnVehiculos = 0.1f;
    public float tiempoSpawnObjetos = 0.1f;

    void Start()
    {
        PrepararObjetos(edificiosPrincipales, out escalasEdificios);
        PrepararObjetos(cuadras, out escalasCuadras);
        PrepararObjetos(npcs, out escalasNPCs);
        PrepararObjetos(vehiculos, out escalasVehiculos);
        PrepararObjetos(objetosUrbanos, out escalasObjetos);
    }

    void PrepararObjetos(GameObject[] objetos, out Vector3[] escalas)
    {
        escalas = new Vector3[objetos.Length];

        for (int i = 0; i < objetos.Length; i++)
        {
            escalas[i] = objetos[i].transform.localScale;
            objetos[i].SetActive(false);
        }
    }

    public void ActivarParte5C()
    {
        StartCoroutine(AparecerParte5C());
    }

    IEnumerator AparecerParte5C()
    {
        if (audioCiudad != null)
        {
            audioCiudad.Play();
        }

        yield return StartCoroutine(
            SpawnGrupo(
                edificiosPrincipales,
                escalasEdificios,
                audioEdificios,
                tiempoSpawnEdificios
            )
        );

        yield return StartCoroutine(
            SpawnGrupo(
                cuadras,
                escalasCuadras,
                audioCuadras,
                tiempoSpawnCuadras
            )
        );

        yield return StartCoroutine(
            SpawnGrupo(
                npcs,
                escalasNPCs,
                audioNPCs,
                tiempoSpawnNPCs
            )
        );

        yield return StartCoroutine(
            SpawnGrupo(
                vehiculos,
                escalasVehiculos,
                audioVehiculos,
                tiempoSpawnVehiculos
            )
        );

        yield return StartCoroutine(
            SpawnGrupo(
                objetosUrbanos,
                escalasObjetos,
                audioObjetosUrbanos,
                tiempoSpawnObjetos
            )
        );
    }

    IEnumerator SpawnGrupo(
        GameObject[] objetos,
        Vector3[] escalas,
        AudioSource audio,
        float duracion
    )
    {
        for (int i = 0; i < objetos.Length; i++)
        {
            if (audio != null)
            {
                audio.PlayOneShot(audio.clip);
            }

            objetos[i].SetActive(true);
            objetos[i].transform.localScale = Vector3.zero;

            yield return StartCoroutine(
                AnimarEscala(
                    objetos[i],
                    escalas[i],
                    duracion
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

            obj.transform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    escalaFinal,
                    tiempo / duracion
                );

            yield return null;
        }

        obj.transform.localScale = escalaFinal;
    }
}