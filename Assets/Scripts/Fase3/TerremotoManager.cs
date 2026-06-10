using UnityEngine;
using System.Collections;

public class TerremotoManager : MonoBehaviour
{
    [Header("Mapa")]
    public Transform mapa;

    [Header("Polvo")]
    public ParticleSystem polvo;

    [Header("NPCs")]
    public GameObject[] npcs;

    [Header("NPCs Nuevos")]
    public GameObject[] npcsNuevos;

    [Header("Ciudad")]
    public GameObject ciudadNormal;
    public GameObject escombros;

    [Header("Audio")]
    public AudioSource audioTerremoto;
    public AudioSource audioSirenas;

    [Header("Configuración")]
    public float duracionTerremoto = 6f;
    public float intensidad = 0.15f;

    void Start()
    {
        if (escombros != null)
        {
            escombros.SetActive(false);
        }

        for (int i = 0; i < npcsNuevos.Length; i++)
        {
            if (npcsNuevos[i] != null)
            {
                npcsNuevos[i].SetActive(false);
            }
        }

        if (audioSirenas != null)
        {
            audioSirenas.Stop();
        }
    }

    public void IniciarTerremoto()
    {
        StartCoroutine(
            SecuenciaTerremoto()
        );
    }

    IEnumerator SecuenciaTerremoto()
    {
        Vector3 posicionOriginal =
            mapa.position;

        // AUDIO TERREMOTO
        if (audioTerremoto != null)
        {
            audioTerremoto.Play();
        }

        float tiempo = 0f;
        bool polvoIniciado = false;

        while (tiempo < duracionTerremoto)
        {
            tiempo += Time.deltaTime;

            // TEMBLOR
            mapa.position =
                posicionOriginal +
                Random.insideUnitSphere *
                intensidad;

            // POLVO DESPUÉS DE 3 SEGUNDOS
            if (
                !polvoIniciado &&
                tiempo >= 3f
            )
            {
                polvoIniciado = true;

                if (polvo != null)
                {
                    polvo.Play();
                }
            }

            yield return null;
        }

        // TERMINA EL TEMBLOR
        mapa.position =
            posicionOriginal;

        // EL POLVO SIGUE 3 SEGUNDOS MÁS
        yield return new WaitForSeconds(3f);

        // NPCs DESAPARECEN DENTRO DEL POLVO
        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i] != null)
            {
                npcs[i].SetActive(false);
            }
        }

        // CAMBIO A ESCOMBROS
        if (ciudadNormal != null)
        {
            ciudadNormal.SetActive(false);
        }

        if (escombros != null)
        {
            escombros.SetActive(true);
        }

        // APARECEN NPCs NUEVOS
        for (int i = 0; i < npcsNuevos.Length; i++)
        {
            if (npcsNuevos[i] != null)
            {
                npcsNuevos[i].SetActive(true);
            }
        }

        // SIRENAS DE EMERGENCIA
        if (audioSirenas != null)
        {
            audioSirenas.Play();
        }

        // EL POLVO CONTINÚA OTRO SEGUNDO
        yield return new WaitForSeconds(3f);
    }
}