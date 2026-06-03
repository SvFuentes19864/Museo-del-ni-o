using UnityEngine;
using System.Collections;

public class InundacionManager : MonoBehaviour
{
    public GameObject agua;

    [Header("NPCs")]
    public GameObject[] npcs;

    public float alturaFinal = 8f;
    public float duracion = 20f;

    void Start()
    {
        if (agua != null)
        {
            agua.SetActive(false);
        }
    }

    public void IniciarInundacion()
    {
        if (agua == null)
        {
            return;
        }

        agua.SetActive(true);

        StartCoroutine(
            DesaparecerNPCsDespues()
        );

        StartCoroutine(SubirAgua());
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