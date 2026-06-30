using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Activa la experiencia cuando hay al menos N avatares de mano presentes
/// durante un tiempo mínimo sostenido.
///
/// Setup en Inspector:
///   1. Arrastra HandAvatarController a "Avatar Controller"
///   2. Ajusta "Manos Para Activar" (1 = cualquier mano, 2 = dos manos, etc.)
///   3. Ajusta "Tiempo Sostenido Seg" — segundos que deben mantenerse las manos
///   4. Conecta "On Activacion Completa" al método que inicia el juego
///   5. (Opcional) Arrastra una Image a "Barra Progreso" para feedback visual
/// </summary>
public class PlayerActivator : MonoBehaviour
{
    [Header("Referencias")]
    public HandAvatarController avatarController;

    [Header("Configuración")]
    [Tooltip("Mínimo de manos activas simultáneas para activar")]
    public int   manosParaActivar   = 2;
    [Tooltip("Segundos que deben sostenerse las manos antes de activar")]
    public float tiempoSostenidoSeg = 2f;

    [Header("Feedback visual (opcional)")]
    [Tooltip("Image que se llena como barra de progreso mientras se sostienen las manos")]
    public Image barraProgreso;

    [Header("Evento")]
    public UnityEvent onActivacionCompleta;

    // ── Estado ────────────────────────────────────────────────────────────────

    private float _timerActivo;
    private bool  _completado;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (barraProgreso != null)
        {
            barraProgreso.type     = Image.Type.Filled;
            barraProgreso.fillAmount = 0f;
        }
    }

    void Update()
    {
        if (_completado) return;

        int manosActivas = ContarManosActivas();
        bool condicion   = manosActivas >= manosParaActivar;

        if (condicion)
        {
            _timerActivo += Time.deltaTime;

            if (barraProgreso != null)
                barraProgreso.fillAmount = Mathf.Clamp01(_timerActivo / tiempoSostenidoSeg);

            if (_timerActivo >= tiempoSostenidoSeg)
            {
                _completado = true;
                if (barraProgreso != null) barraProgreso.fillAmount = 1f;
                onActivacionCompleta?.Invoke();
            }
        }
        else
        {
            // Resetear si las manos se retiran antes del tiempo
            _timerActivo = 0f;
            if (barraProgreso != null)
                barraProgreso.fillAmount = 0f;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    int ContarManosActivas()
    {
        int count = 0;
        foreach (var av in avatarController.avatares)
            if (av != null && av.gameObject.activeSelf)
                count++;
        return count;
    }

    public void Reiniciar()
    {
        _completado  = false;
        _timerActivo = 0f;
        if (barraProgreso != null) barraProgreso.fillAmount = 0f;
    }
}
