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

    [Header("Ciudad")]
    public GameObject ciudadNormal;
    public GameObject escombros;

    [Header("Audio")]
    public AudioSource audioTerremoto;

    [Header("Configuración")]
    public float duracionTerremoto = 6f;
    public float intensidad = 0.15f;

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

        // AUDIO
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

        // EL POLVO SIGUE 1 SEGUNDO MÁS
        yield return new WaitForSeconds(3f);

        // NPCs DESAPARECEN DENTRO DEL POLVO
        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i] != null)
            {
                npcs[i].SetActive(false);
            }
        }

        // CAMBIO A ESCOMBROS MIENTRAS HAY POLVO
        if (ciudadNormal != null)
        {
            ciudadNormal.SetActive(false);
        }

        if (escombros != null)
        {
            escombros.SetActive(true);
        }

        // EL POLVO CONTINÚA OTRO SEGUNDO
        yield return new WaitForSeconds(3f);
    }
}