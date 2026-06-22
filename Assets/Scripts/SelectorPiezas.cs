using UnityEngine;
using System.Collections;

public class SelectorArco : MonoBehaviour
{
    [Header("Posiciones")]
    public Transform[] posiciones;

    [Header("Ruleta")]
    public int saltosMinimos = 6;
    public int saltosMaximos = 10;

    [Header("Velocidad")]
    public float tiempoEntreSaltos = 0.15f;

    [Header("Fade")]
    public float duracionFade = 0.07f;

    [Header("Audio")]
    public AudioSource audioRuleta;
    public AudioSource audioGanador;

    [Header("Offset Visual")]
    public Vector3 offsetVisual;
    public OrbbecUnity.OrbbecDevice orbbecDevice;

    private int ultimoGanador = -1;
    private int ultimoIndiceMostrado = -1;

    private MeshRenderer meshRendererArco;
    private Material materialArco;

    private Quaternion rotacionOriginal;
    private float alturaOriginal;

    void Start()
    {
        rotacionOriginal = transform.rotation;
        alturaOriginal = transform.position.y;

        meshRendererArco = GetComponent<MeshRenderer>();

        if (meshRendererArco != null)
        {
            materialArco = meshRendererArco.material;

            Color color = materialArco.color;
            color.a = 0f;
            materialArco.color = color;
        }
    }

    public void IniciarRuleta()
    {
        StartCoroutine(Ruleta());
    }

    IEnumerator EncenderOrbbec()
    {
        yield return new WaitForSeconds(1f);

        if (orbbecDevice != null)
        {
            orbbecDevice.enabled = true;

            Debug.Log("ORBBEC ACTIVADO");
        }
    }

    IEnumerator Ruleta()
    {
        if (posiciones == null || posiciones.Length == 0)
        {
            Debug.LogWarning("No hay posiciones asignadas.");
            yield break;
        }

        int ganador;

        do
        {
            ganador = Random.Range(0, posiciones.Length);
        }
        while (
            posiciones.Length > 1 &&
            ganador == ultimoGanador
        );

        ultimoGanador = ganador;

        int totalSaltos =
            Random.Range(
                saltosMinimos,
                saltosMaximos + 1
            );

        for (int salto = 0; salto < totalSaltos; salto++)
        {
            int indiceAleatorio;

            do
            {
                indiceAleatorio =
                    Random.Range(
                        0,
                        posiciones.Length
                    );
            }
            while (
                posiciones.Length > 1 &&
                indiceAleatorio ==
                ultimoIndiceMostrado
            );

            ultimoIndiceMostrado =
                indiceAleatorio;

            transform.position =
                new Vector3(
                    posiciones[indiceAleatorio].position.x,
                    alturaOriginal,
                    posiciones[indiceAleatorio].position.z
                ) + offsetVisual;

            transform.rotation =
                rotacionOriginal;

            if (audioRuleta != null)
            {
                audioRuleta.Play();
            }

            yield return StartCoroutine(
                FadeIn()
            );

            yield return new WaitForSeconds(
                tiempoEntreSaltos
            );

            yield return StartCoroutine(
                FadeOut()
            );
        }

        transform.position =
            new Vector3(
                posiciones[ganador].position.x,
                alturaOriginal,
                posiciones[ganador].position.z
            ) + offsetVisual;

        transform.rotation =
            rotacionOriginal;

        if (audioRuleta != null)
        {
            audioRuleta.PlayOneShot(
                audioRuleta.clip
            );
        }

        yield return StartCoroutine(
            FadeIn()
        );

        if (audioGanador != null)
        {
            audioGanador.PlayOneShot(
                audioGanador.clip
            );
        }

        // DESACTIVAR TODOS LOS PUNTOS Y SUS HIJOS
        for (int i = 0; i < posiciones.Length; i++)
        {
            if (posiciones[i] != null)
            {
                posiciones[i].gameObject.SetActive(false);
            }
        }

        Debug.Log(
            "Jugador seleccionado: " +
            (ganador + 1)
        );

        StartCoroutine(
            EncenderOrbbec()
        );
    }

    IEnumerator FadeIn()
    {
        if (materialArco == null)
        {
            yield break;
        }

        Color color = materialArco.color;

        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;

            color.a =
                Mathf.Lerp(
                    0f,
                    1f,
                    tiempo / duracionFade
                );

            materialArco.color = color;

            yield return null;
        }

        color.a = 1f;
        materialArco.color = color;
    }

    IEnumerator FadeOut()
    {
        if (materialArco == null)
        {
            yield break;
        }

        Color color = materialArco.color;

        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;

            color.a =
                Mathf.Lerp(
                    1f,
                    0f,
                    tiempo / duracionFade
                );

            materialArco.color = color;

            yield return null;
        }

        color.a = 0f;
        materialArco.color = color;
    }
}