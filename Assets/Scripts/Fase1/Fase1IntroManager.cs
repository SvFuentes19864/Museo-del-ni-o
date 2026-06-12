using UnityEngine;
using System.Collections;

public class Fase1IntroManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narrador;

    [Header("Ruleta")]
    public SelectorArco selectorArco;

    IEnumerator Start()
    {
        if (narrador != null)
        {
            narrador.Play();

            yield return new WaitForSeconds(
                narrador.clip.length
            );
        }

        if (selectorArco != null)
        {
            selectorArco.IniciarRuleta();
        }
    }
}