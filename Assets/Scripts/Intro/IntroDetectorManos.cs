using UnityEngine;
using UnityEngine.UI;

public class IntroDetectorManos : MonoBehaviour
{
    [Header("Inicio")]
    public IntroManager introManager;

    [Header("Configuración")]
    public int   manosNecesarias  = 2;
    public float tiempoParaIniciar = 3f;
    [Tooltip("Distancia máxima (px canvas) entre avatar y silueta para contar como 'encima'")]
    public float radioActivacion  = 80f;

    [Header("Tracker de avatares")]
    public HandAvatarController avatarController;

    [Header("Siluetas")]
    public Image[] siluetas;

    // ── Estado ────────────────────────────────────────────────────────────────

    private bool  _juegoIniciado;
    private float _timerInicio;

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (avatarController == null || _juegoIniciado) return;

        int siluetasActivadas = ContarSiluetasOcupadas();

        // Feedback visual: silueta opaca si tiene un avatar encima
        ActualizarSiluetas();

        // Timer
        if (siluetasActivadas >= manosNecesarias)
        {
            _timerInicio += Time.deltaTime;
        }
        else
        {
            _timerInicio = 0f;
        }

        // Iniciar
        if (_timerInicio >= tiempoParaIniciar)
        {
            _juegoIniciado = true;

            foreach (var img in siluetas)
                img?.gameObject.SetActive(false);

            introManager?.IniciarIntro();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    int ContarSiluetasOcupadas()
    {
        int count = 0;
        foreach (var silueta in siluetas)
        {
            if (silueta == null) continue;
            if (AvatarEncima(silueta.rectTransform)) count++;
        }
        return count;
    }

    bool AvatarEncima(RectTransform silueta)
    {
        foreach (var av in avatarController.avatares)
        {
            if (av == null || !av.gameObject.activeSelf) continue;
            if (Vector3.Distance(silueta.position, av.position) <= radioActivacion)
                return true;
        }
        return false;
    }

    void ActualizarSiluetas()
    {
        // Marcar individualmente cuál silueta tiene avatar encima
        for (int i = 0; i < siluetas.Length; i++)
        {
            if (siluetas[i] == null) continue;
            bool ocupada = AvatarEncima(siluetas[i].rectTransform);
            Color c = siluetas[i].color;
            c.a = ocupada ? 1f : 0.2f;
            siluetas[i].color = c;
        }
    }
}
