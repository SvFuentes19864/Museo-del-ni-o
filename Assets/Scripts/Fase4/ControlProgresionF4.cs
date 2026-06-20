using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

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

    [Header("Narraciones")]
    public AudioSource introF4;
    public AudioSource finalF4;

    [Header("Cámaras")]
    public CinemachineCamera camaraPrincipal;

    public CinemachineCamera camaraParte1;

    public CinemachineCamera camaraParte2A;
    public CinemachineCamera camaraParte2B;

    public CinemachineCamera camaraParte3;

    public CinemachineCamera camaraParte4;

    public CinemachineCamera camaraParte5A;
    public CinemachineCamera camaraParte5B;
    public CinemachineCamera camaraParte5C;

    public CinemachineCamera camaraZoomOut;

    public void MostrarIglesiaRecoleccion()
    {

        ActivarCamara(camaraParte2A);

        if (iglesiaRecoleccion != null)
        {
            iglesiaRecoleccion.SetActive(true);
        }
    }

    public void MostrarParqueJocotenango()
    {

        ActivarCamara(camaraParte2B);

        if (parqueJocotenango != null)
        {
            parqueJocotenango.SetActive(true);
        }
    }

    public void MostrarAcueducto()
    {

        ActivarCamara(camaraParte3);

        if (acueducto != null)
        {
            acueducto.SetActive(true);
        }
    }

    public void MostrarIglesiaSantoDomingo()
    {

        ActivarCamara(camaraParte4);

        if (iglesiaSantoDomingo != null)
        {
            iglesiaSantoDomingo.SetActive(true);
        }
    }

    public void MostrarMunicipalidad()
    {

        ActivarCamara(camaraParte5A);

        if (municipalidad != null)
        {
            municipalidad.SetActive(true);
        }
    }

    public void MostrarTorreReformador()
    {

        ActivarCamara(camaraParte5B);

        if (torreReformador != null)
        {
            torreReformador.SetActive(true);
        }
    }

    public void MostrarPEspana()
    {

        ActivarCamara(camaraParte5C);

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

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            iglesiaRecoleccion != null
        )
        {
            iglesiaRecoleccion.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                iglesiaRecoleccion,
                inicioDragIglesiaRecoleccion,
                camaraParte2A
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator EsperarYMostrarParqueJocotenango()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            parqueJocotenango != null
        )
        {
            parqueJocotenango.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                parqueJocotenango,
                inicioDragParqueJocotenango,
                camaraParte2B
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator EsperarYMostrarAcueducto()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            acueducto != null
        )
        {
            acueducto.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                acueducto,
                inicioDragAcueducto,
                camaraParte3
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator EsperarYMostrarIglesiaSantoDomingo()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            iglesiaSantoDomingo != null
        )
        {
            iglesiaSantoDomingo.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                iglesiaSantoDomingo,
                inicioDragIglesiaSantoDomingo,
                camaraParte4
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator EsperarYMostrarMunicipalidad()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            municipalidad != null
        )
        {
            municipalidad.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                municipalidad,
                inicioDragMunicipalidad,
                camaraParte5A
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator EsperarYMostrarTorreReformador()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            torreReformador != null
        )
        {
            torreReformador.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                torreReformador,
                inicioDragTorreReformador,
                camaraParte5B
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator EsperarYMostrarPEspana()
    {
        yield return new WaitForSeconds(
            tiempoEntrePartes
        );

        ActivarCamara(camaraZoomOut);

        if (
            controlRuletaF4 != null &&
            pEspana != null
        )
        {
            pEspana.SetActive(true);

            controlRuletaF4.ConfigurarRuleta(
                pEspana,
                inicioDragPEspana,
                camaraParte5C
            );

            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator IntroInicialF4()
    {
        ActivarCamara(camaraPrincipal);

        if (introF4 != null)
        {
            introF4.Play();

            yield return new WaitForSeconds(
                introF4.clip.length
            );
        }

        ActivarCamara(camaraZoomOut);

        if (cerrito != null)
        {
            cerrito.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        if (controlRuletaF4 != null)
        {
            controlRuletaF4.IniciarRuleta();
        }
    }

    IEnumerator NarracionFinalF4()
    {
        ActivarCamara(camaraPrincipal);

        if (finalF4 != null)
        {
            finalF4.Play();

            yield return new WaitForSeconds(
                delayAntesRecorridoFinal
            );

            ActivarCamara(camaraCart);

            yield return new WaitForSeconds(
                finalF4.clip.length -
                delayAntesRecorridoFinal
            );
        }

        Debug.Log(
            "Narración final F4 terminada."
        );
    }

    //SEPARADOR

    [Header("Tiempos")]
    public float tiempoEntrePartes = 3f;

    [Header("Cinemática Final")]
    public float delayAntesRecorridoFinal = 3f;

    [Header("Ruleta Inicial")]
    public ControlRuletaF4 controlRuletaF4;
    [Header("Puntos Inicio Drag")]
    public Transform inicioDragCerrito;
    public Transform inicioDragIglesiaRecoleccion;
    public Transform inicioDragParqueJocotenango;
    public Transform inicioDragAcueducto;
    public Transform inicioDragIglesiaSantoDomingo;
    public Transform inicioDragMunicipalidad;
    public Transform inicioDragTorreReformador;
    public Transform inicioDragPEspana;

    public CinemachineCamera camaraCart;

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

    void ActivarCamara(
        CinemachineCamera camaraActiva
    )
    {
        camaraParte1.Priority = 0;

        camaraParte2A.Priority = 0;
        camaraParte2B.Priority = 0;

        camaraParte3.Priority = 0;

        camaraParte4.Priority = 0;

        camaraParte5A.Priority = 0;
        camaraParte5B.Priority = 0;
        camaraParte5C.Priority = 0;

        camaraPrincipal.Priority = 0;

        camaraZoomOut.Priority = 0;

        camaraCart.Priority = 0;

        if (camaraActiva != null)
        {
            camaraActiva.Priority = 100;
        }
    }
}