using UnityEngine;
using UnityEngine.UI;

public class IntroDetectorManos : MonoBehaviour
{
    [Header("Inicio")]
    public IntroManager introManager;

    private bool juegoIniciado = false;

    [Header("Configuración")]
    public int manosNecesarias = 2;
    public float tiempoParaIniciar = 3f;

    private float timerInicio = 0f;

    [Header("Tracker")]
    public IntroHandTracker handTracker;

    [Header("Siluetas")]
    public Image[] siluetas;

    void Update()
    {
        if (handTracker == null || juegoIniciado)
            return;

        int manos =
            handTracker.cantidadManosDetectadas;

        // Actualizar siluetas
        for (int i = 0; i < siluetas.Length; i++)
        {
            if (siluetas[i] == null)
                continue;

            Color c =
                siluetas[i].color;

            c.a =
                i < manos
                ? 1f
                : 0.2f;

            siluetas[i].color = c;
        }

        // Contador de inicio
        if (manos >= manosNecesarias)
        {
            timerInicio += Time.deltaTime;

            float restante =
                Mathf.Max(
                    0f,
                    tiempoParaIniciar - timerInicio
                );

            Debug.Log(
                "Iniciando en: " +
                restante.ToString("F1")
            );
        }
        else
        {
            timerInicio = 0f;
        }

        // Iniciar juego
        if (
            timerInicio >= tiempoParaIniciar &&
            !juegoIniciado
        )
        {
            juegoIniciado = true;

            foreach (Image img in siluetas)
            {
                if (img != null)
                {
                    img.gameObject.SetActive(false);
                }
            }

            Debug.Log(
                "INICIAR JUEGO"
            );

            if (introManager != null)
            {
                introManager.IniciarIntro();
            }
        }
    }
}