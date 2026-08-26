using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HandTrackerF4 : MonoBehaviour
{
    [Header("Servidor Python")]
    public string pythonExecutable =
        @"C:\Users\alane\OneDrive\Escritorio\Tracking Server\.venv\Scripts\python.exe";
    public string pythonScriptPath =
        @"C:\Users\alane\OneDrive\Escritorio\Tracking Server\main.py";

    [Header("Conexión TCP")]
    public string serverHost        = "127.0.0.1";
    public int    serverPort        = 9000;
    public float  reconnectDelaySec = 2f;

    [Header("Plano 3D")]
    [Tooltip("Altura Y del plano sobre el que se proyecta la mano")]
    public float alturaPlanoY = 0f;
    public Vector3 offsetVisual3DF4 = Vector3.zero;

    [Header("Avatares 2D")]
    public RectTransform[] avatares;
    public RectTransform areaCanvas;
    public float deadZonePx      = 6f;
    public float suavizadoAvatares = 10f;

    // Posición 3D, posición en viewport (0-1) y estado de press de TODAS las manos.
    // HandDraggableF4 lee estos diccionarios para decidir cuál mano la controla.
    [HideInInspector] public readonly Dictionary<int, Vector3> handWorldPositions    = new();
    [HideInInspector] public readonly Dictionary<int, Vector2> handViewportPositions = new();
    [HideInInspector] public readonly Dictionary<int, bool>    handPressedStates     = new();

    // Manos ya reclamadas por algún HandDraggableF4, para que una mano no mueva dos objetos a la vez.
    [HideInInspector] public readonly HashSet<int> manosReclamadas = new();

    private readonly Dictionary<int, int>   _manoAAvatar = new();
    private bool[]    _avatarEnUso;
    private Vector2[] _targetPos;

    private static Process s_trackerProcess;
    private TcpClient     _client;
    private Thread        _thread;
    private volatile bool _running;
    private FrameData     _pendingFrame;
    private readonly object _lock = new();

    [Serializable] private class PointerData { public int id; public float x, y; public string state; public string side; }
    [Serializable] private class FrameData   { public string type; public int frame_id; public PointerData[] pointers; }

    // Llamado por ControlRuletaF4 al asignar un nuevo objeto activo.
    // Solo actualiza la altura del plano de proyección.
    public void CambiarObjetoF4(Transform nuevoObjeto)
    {
        if (nuevoObjeto != null)
            alturaPlanoY = nuevoObjeto.position.y;
    }

    void Start()
    {
        _avatarEnUso = new bool[avatares?.Length ?? 0];
        _targetPos   = new Vector2[avatares?.Length ?? 0];
        if (avatares != null)
            foreach (var av in avatares)
                if (av) av.gameObject.SetActive(false);

        LaunchServer();
        _running = true;
        _thread  = new Thread(ReceiveLoop) { IsBackground = true };
        _thread.Start();
        Application.quitting += KillServer;
    }

    void LaunchServer()
    {
        if (s_trackerProcess != null && !s_trackerProcess.HasExited)
        {
            UnityEngine.Debug.Log("[HandTrackerF4] Servidor ya en ejecución — reutilizando.");
            return;
        }
        try
        {
            s_trackerProcess = Process.Start(new ProcessStartInfo
            {
                FileName         = pythonExecutable,
                Arguments        = $"\"{pythonScriptPath}\"",
                WorkingDirectory = Path.GetDirectoryName(pythonScriptPath),
                CreateNoWindow   = true,
                UseShellExecute  = false,
            });
            UnityEngine.Debug.Log("[HandTrackerF4] Servidor lanzado.");
        }
        catch (Exception e) { UnityEngine.Debug.LogError("[HandTrackerF4] " + e.Message); }
    }

    static void KillServer()
    {
        if (s_trackerProcess == null || s_trackerProcess.HasExited) return;
        try { s_trackerProcess.Kill(); s_trackerProcess = null; } catch { }
    }

    void ReceiveLoop()
    {
        while (_running)
        {
            try
            {
                _client = new TcpClient(serverHost, serverPort);
                UnityEngine.Debug.Log("[HandTrackerF4] Conectado al servidor TCP.");
                var stream = _client.GetStream();
                var buffer = new byte[8192];
                var sb     = new StringBuilder();

                while (_running)
                {
                    int n = stream.Read(buffer, 0, buffer.Length);
                    if (n == 0) break;
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, n));

                    string raw; int idx;
                    while ((idx = (raw = sb.ToString()).IndexOf('\n')) >= 0)
                    {
                        string line = raw[..idx].Trim();
                        sb.Remove(0, idx + 1);
                        if (line.Length == 0) continue;
                        var frame = JsonUtility.FromJson<FrameData>(line);
                        if (frame != null)
                            lock (_lock) { _pendingFrame = frame; }
                    }
                }
            }
            catch (Exception e)
            {
                if (_running)
                    UnityEngine.Debug.LogWarning($"[HandTrackerF4] Desconectado — {e.Message}. Reintentando en {reconnectDelaySec}s…");
            }
            finally { _client?.Close(); _client = null; }

            if (_running) Thread.Sleep((int)(reconnectDelaySec * 1000));
        }
    }

    void Update()
    {
        FrameData frame;
        lock (_lock) { frame = _pendingFrame; _pendingFrame = null; }
        if (frame?.pointers != null)
            AplicarManosF4(frame.pointers);

        // suavizado de avatares
        float t = suavizadoAvatares <= 0f ? 1f : 1f - Mathf.Exp(-suavizadoAvatares * Time.deltaTime);
        if (avatares != null)
            for (int i = 0; i < avatares.Length; i++)
                if (_avatarEnUso[i] && avatares[i] != null)
                    avatares[i].anchoredPosition = Vector2.Lerp(avatares[i].anchoredPosition, _targetPos[i], t);
    }

    void AplicarManosF4(PointerData[] pointers)
    {
        var vistas = new HashSet<int>();

        foreach (var p in pointers)
        {
            if (p.state == "up")
            {
                RemoveHand(p.id);
                continue;
            }

            vistas.Add(p.id);

            // avatar 2D
            if (!_manoAAvatar.TryGetValue(p.id, out int avIdx))
            {
                avIdx = PrimerAvatarLibre();
                if (avIdx >= 0)
                {
                    _avatarEnUso[avIdx]  = true;
                    _manoAAvatar[p.id]   = avIdx;
                    _targetPos[avIdx]    = AvatarPos(p.x, p.y);
                    avatares[avIdx].anchoredPosition = _targetPos[avIdx];
                    avatares[avIdx].gameObject.SetActive(true);
                }
            }
            else
            {
                Vector2 next = AvatarPos(p.x, p.y);
                if (Vector2.Distance(_targetPos[avIdx], next) > deadZonePx)
                    _targetPos[avIdx] = next;
            }

            // datos públicos para HandDraggableF4
            handPressedStates[p.id]     = p.state != "up";
            handViewportPositions[p.id] = new Vector2(p.x, 1f - p.y);

            if (Camera.main != null)
            {
                Vector3 screenPos = new Vector3(p.x * Screen.width, (1f - p.y) * Screen.height, 0f);
                Plane plane = new Plane(Vector3.up, new Vector3(0, alturaPlanoY, 0));
                Ray ray = Camera.main.ScreenPointToRay(screenPos);
                if (plane.Raycast(ray, out float dist))
                    handWorldPositions[p.id] = ray.GetPoint(dist) + offsetVisual3DF4;
            }
        }

        // limpiar manos perdidas sin "up" explícito
        foreach (int key in new List<int>(_manoAAvatar.Keys))
            if (!vistas.Contains(key)) RemoveHand(key);
    }

    void RemoveHand(int id)
    {
        if (_manoAAvatar.TryGetValue(id, out int avIdx))
        {
            avatares[avIdx].gameObject.SetActive(false);
            _avatarEnUso[avIdx] = false;
            _manoAAvatar.Remove(id);
        }
        handWorldPositions.Remove(id);
        handViewportPositions.Remove(id);
        handPressedStates.Remove(id);
    }

    Vector2 AvatarPos(float nx, float ny)
    {
        if (areaCanvas == null) return Vector2.zero;
        Rect r = areaCanvas.rect;
        return new Vector2((nx - 0.5f) * r.width, (0.5f - ny) * r.height);
    }

    int PrimerAvatarLibre()
    {
        if (avatares == null) return -1;
        for (int i = 0; i < avatares.Length; i++)
            if (!_avatarEnUso[i] && avatares[i] != null) return i;
        return -1;
    }

    void OnDestroy()
    {
        _running = false;
        _client?.Close();
        if (avatares != null)
            foreach (var av in avatares)
                if (av) av.gameObject.SetActive(false);
    }
}
