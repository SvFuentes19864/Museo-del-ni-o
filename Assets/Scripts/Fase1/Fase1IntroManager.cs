using UnityEngine;
using System.Collections;

public class Fase1IntroManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource narrador;

    [Header("Ruleta")]
    public SelectorArco selectorArco;

    private MeshRenderer[] renderersArco;

    IEnumerator Start()
    {
        if (selectorArco != null)
        {
            renderersArco =
                selectorArco.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer r in renderersArco)
            {
                r.enabled = false;
            }
        }

        if (narrador != null)
        {
            narrador.Play();

            yield return new WaitForSeconds(
                narrador.clip.length
            );
        }

        if (renderersArco != null)
        {
            foreach (MeshRenderer r in renderersArco)
            {
                r.enabled = true;
            }
        }

        if (selectorArco != null)
        {
            selectorArco.IniciarRuleta();
        }
    }
}