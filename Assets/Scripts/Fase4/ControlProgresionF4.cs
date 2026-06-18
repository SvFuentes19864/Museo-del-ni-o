using UnityEngine;
using System.Collections;

public class ControlProgresionF4 : MonoBehaviour
{
    [Header("Encajables")]
    public GameObject cerrito;

    public GameObject iglesiaRecoleccion;
    public GameObject parqueJocotenango;

    public GameObject acueducto;

    public GameObject iglesiaSantoDomingo;

    public GameObject municipalidad;
    public GameObject torreReformador;
    public GameObject pEspana;

    [Header("Narración Inicial")]
    public AudioSource introF4;
    public AudioSource finalF4;

    public void MostrarIglesiaRecoleccion()
    {
        if (iglesiaRecoleccion != null)
        {
            iglesiaRecoleccion.SetActive(true);
        }
    }

    public void MostrarParqueJocotenango()
    {
        if (parqueJocotenango != null)
        {
            parqueJocotenango.SetActive(true);
        }
    }

    public void MostrarAcueducto()
    {
        if (acueducto != null)
        {
            acueducto.SetActive(true);
        }
    }

    public void MostrarIglesiaSantoDomingo()
    {
        if (iglesiaSantoDomingo != null)
        {
            iglesiaSantoDomingo.SetActive(true);
        }
    }

    public void MostrarMunicipalidad()
    {
        if (municipalidad != null)
        {
            municipalidad.SetActive(true);
        }
    }

    public void MostrarTorreReformador()
    {
        if (torreReformador != null)
        {
            torreReformador.SetActive(true);
        }
    }

    public void MostrarPEspana()
    {
        if (pEspana != null)
        {
            pEspana.SetActive(true);
        }
    }
    
    //SEPARADOR

    public void MostrarIglesiaRecoleccionConDelay()
    {
        StartCoroutine(
            EsperarYMostrarIglesiaRecoleccion()
        );
    }

    public void MostrarParqueJocotenangoConDelay()
    {
        StartCoroutine(
            EsperarYMostrarParqueJocotenango()
        );
    }

    public void MostrarAcueductoConDelay()
    {
        StartCoroutine(
            EsperarYMostrarAcueducto()
        );
    }

    public void MostrarIglesiaSantoDomingoConDelay()
    {
        StartCoroutine(
            EsperarYMostrarIglesiaSantoDomingo()
        );
    }

    public void MostrarMunicipalidadConDelay()
    {
        StartCoroutine(
            EsperarYMostrarMunicipalidad()
        );
    }

    public void MostrarTorreReformadorConDelay()
    {
        StartCoroutine(
            EsperarYMostrarTorreReformador()
        );
    }

    public void MostrarPEspanaConDelay()
    {
        StartCoroutine(
            EsperarYMostrarPEspana()
        );
    }

    public void ReproducirFinalF4()
    {
        StartCoroutine(
            NarracionFinalF4()
        );
    }

    //SEPARADOR

    IEnumerator EsperarYMostrarIglesiaRecoleccion()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarIglesiaRecoleccion();
    }

    IEnumerator EsperarYMostrarParqueJocotenango()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarParqueJocotenango();
    }

    IEnumerator EsperarYMostrarAcueducto()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarAcueducto();
    }

    IEnumerator EsperarYMostrarIglesiaSantoDomingo()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarIglesiaSantoDomingo();
    }

    IEnumerator EsperarYMostrarMunicipalidad()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarMunicipalidad();
    }

    IEnumerator EsperarYMostrarTorreReformador()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarTorreReformador();
    }

    IEnumerator EsperarYMostrarPEspana()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        MostrarPEspana();
    }

    IEnumerator IntroInicialF4()
    {
        if (introF4 != null)
        {
            introF4.Play();

            yield return new WaitForSeconds(
                introF4.clip.length
            );
        }

        if (cerrito != null)
        {
            cerrito.SetActive(true);
        }
    }

    IEnumerator NarracionFinalF4()
    {
        if (finalF4 != null)
        {
            finalF4.Play();

            yield return new WaitForSeconds(
                finalF4.clip.length
            );
        }

        Debug.Log(
            "Narración final F4 terminada."
        );
    }

    //SEPARADOR

    [Header("Tiempos")]
    public float tiempoEntrePartes = 3f;

    void Start()
    {
        if (cerrito != null)
        {
            cerrito.SetActive(false);
        }

        if (iglesiaRecoleccion != null)
        {
            iglesiaRecoleccion.SetActive(false);
        }

        if (parqueJocotenango != null)
        {
            parqueJocotenango.SetActive(false);
        }

        if (acueducto != null)
        {
            acueducto.SetActive(false);
        }

        if (iglesiaSantoDomingo != null)
        {
            iglesiaSantoDomingo.SetActive(false);
        }

        if (municipalidad != null)
        {
            municipalidad.SetActive(false);
        }

        if (torreReformador != null)
        {
            torreReformador.SetActive(false);
        }

        if (pEspana != null)
        {
            pEspana.SetActive(false);
        }

        StartCoroutine(
            IntroInicialF4()
        );
    }
}