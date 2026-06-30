using UnityEngine;
using System.Collections.Generic;

public class HandAvatarController : MonoBehaviour
{
    [Header("Tracker")]
    public IntroHandTracker handTracker;

    [Header("Sprites / avatares  (arrastra los RectTransform)")]
    public RectTransform[] avatares;

    [Header("Área del Canvas que cubre la mesa")]
    public RectTransform areaCanvas;

    [Header("Estabilizador")]
    [Tooltip("Qué tan rápido sigue la mano. Menor = más suave pero más lag. Recomendado: 8-14")]
    public float smoothSpeed = 10f;
    [Tooltip("Movimiento mínimo (px canvas) para actualizar el target. Elimina jitter estacionario.")]
    public float deadZonePx = 6f;

    // ── Estado interno ────────────────────────────────────────────────────────

    private readonly Dictionary<int, int> _manoAAvatar = new();
    private bool[]    _avatarEnUso;
    private Vector2[] _targetPos;      // destino actualizado por TCP (~26 fps)

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        _avatarEnUso = new bool[avatares.Length];
        _targetPos   = new Vector2[avatares.Length];

        foreach (var av in avatares)
            if (av) av.gameObject.SetActive(false);

        handTracker.OnHandDown += OnManoAbajo;
        handTracker.OnHandMove += OnManoMueve;
        handTracker.OnHandUp   += OnManoSube;
    }

    void OnDestroy()
    {
        if (handTracker == null) return;
        handTracker.OnHandDown -= OnManoAbajo;
        handTracker.OnHandMove -= OnManoMueve;
        handTracker.OnHandUp   -= OnManoSube;
    }

    // Update corre a 60 fps: lerp suave desde posición actual hacia _targetPos
    void Update()
    {
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        for (int i = 0; i < avatares.Length; i++)
        {
            if (!_avatarEnUso[i] || avatares[i] == null) continue;
            avatares[i].anchoredPosition = Vector2.Lerp(
                avatares[i].anchoredPosition, _targetPos[i], t);
        }
    }

    // ── Eventos TCP (~26 fps) ─────────────────────────────────────────────────

    void OnManoAbajo(IntroHandTracker.PointerInfo p)
    {
        int idx = PrimerAvatarLibre();
        if (idx < 0) return;

        _avatarEnUso[idx]  = true;
        _manoAAvatar[p.id] = idx;

        // En down: teletransportar al punto inicial sin lerp
        _targetPos[idx] = CanvasPos(p.x, p.y);
        avatares[idx].anchoredPosition = _targetPos[idx];
        avatares[idx].gameObject.SetActive(true);
    }

    void OnManoMueve(IntroHandTracker.PointerInfo p)
    {
        if (!_manoAAvatar.TryGetValue(p.id, out int idx)) return;

        Vector2 next = CanvasPos(p.x, p.y);
        // Dead zone: ignorar micro-movimientos para eliminar jitter estacionario
        if (Vector2.Distance(_targetPos[idx], next) > deadZonePx)
            _targetPos[idx] = next;
    }

    void OnManoSube(IntroHandTracker.PointerInfo p)
    {
        if (!_manoAAvatar.TryGetValue(p.id, out int idx)) return;

        avatares[idx].gameObject.SetActive(false);
        _avatarEnUso[idx] = false;
        _manoAAvatar.Remove(p.id);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Vector2 CanvasPos(float nx, float ny)
    {
        if (areaCanvas == null) return Vector2.zero;
        Rect r = areaCanvas.rect;
        return new Vector2(
            (nx - 0.5f) * r.width,
            (0.5f - ny) * r.height
        );
    }

    int PrimerAvatarLibre()
    {
        for (int i = 0; i < avatares.Length; i++)
            if (!_avatarEnUso[i] && avatares[i] != null)
                return i;
        return -1;
    }
}
