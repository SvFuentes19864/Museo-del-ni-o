using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HandTracker : MonoBehaviour
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

    [Header("Objetos 3D a mover")]
    public List<Transform> handSpheres = new();
    [Tooltip("Usa esto si los objetos necesitan un pequeño empujón en X o Z para verse centrados debajo del cursor")]
    public Vector3 offsetVisual3D = Vector3.zero;

    [Header("Avatares 2D")]
    [Tooltip("Arrastra aquí los RectTransform de los avatares de jugadores (igual que en la Intro)")]
    public RectTransform[] avatares;
    [Tooltip("RectTransform del área del Canvas que cubre la mesa")]
    public RectTransform areaCanvas;
    [Tooltip("Movimiento mínimo (px canvas) para actualizar la posición. Reduce jitter estacionario.")]
    public float deadZonePx = 6f;

    [Header("Reclamación de objeto")]
    [Tooltip("Radio (metros) dentro del cual la mano puede reclamar un objeto")]
    public float radioDeReclamacion = 1f;
    [Tooltip("Segundos que la mano debe estar dentro del radio antes de poder mover el objeto")]
    public float tiempoParaReclamar = 1f;

    [Header("Suavizado de movimiento")]
    [Tooltip("Suavizado de avatares 2D y esferas 3D. 10 = igual que la Intro. Sube para más respuesta.")]
    public float suavizadoAvatares = 10f;

    [HideInInspector] public Vector2 handPositionNormalized;
    [HideInInspector] public float handDepth;
    [HideInInspector] public bool handPressed;

    private float alturaFijaY;
    private bool lastHandPressed;

    private readonly Dictionary<int, int>   _manoAAvatar  = new(); // hand.id → índice en avatares[]
    private bool[]    _avatarEnUso;
    private Vector2[] _targetPos;                                   // destino suavizado de cada avatar

    private readonly Dictionary<int, int>   handToSphere  = new();
    private readonly Dictionary<int, int>   hoverTarget   = new();
    private readonly Dictionary<int, float> hoverTimer    = new();
    private readonly Dictionary<int, int>   missingFrames = new();
    private readonly Dictionary<int, Vector3> sphereTarget = new();

    [Header("Estabilidad de tracking")]
    [Tooltip("Frames consecutivos sin detectar la mano antes de considerarla desaparecida")]
    public int gracePeriodFrames = 5;
    [Tooltip("Ocultar la esfera 3D cuando la mano desaparece. Desactivar si la esfera es un objeto del juego (no solo cursor)")]
    public bool ocultarEsferaAlPerder = true;

    private static Process s_trackerProcess;

    private TcpClient     _client;
    private Thread        _thread;
    private volatile bool _running;
    private FrameData     _pendingFrame;
    private readonly object _lock = new();

    [Serializable] private class PointerData
    {
        public int    id;
        public float  x, y;
        public string state;  // "down" | "move" | "up"
        public string side;   // "L" | "R"
    }

    [Serializable] private class FrameData
    {
        public string        type;
        public int           frame_id;
        public PointerData[] pointers;
    }

    void Start()
    {
        if (handSpheres.Count > 0 && handSpheres[0] != null)
            alturaFijaY = handSpheres[0].position.y;

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
        UnityEngine.Debug.Log($"[HandTracker] LISTO en escena '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' — esferas: {handSpheres.Count}, radioReclamacion: {radioDeReclamacion}");
    }

    void LaunchServer()
    {
        if (s_trackerProcess != null && !s_trackerProcess.HasExited)
        {
            UnityEngine.Debug.Log("[HandTracker] Servidor ya en ejecución — reutilizando.");
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
            UnityEngine.Debug.Log("[HandTracker] Servidor lanzado: " + pythonScriptPath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[HandTracker] No se pudo lanzar el servidor: " + e.Message);
        }
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
                UnityEngine.Debug.Log("[HandTracker] Conectado al servidor TCP.");
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
                    UnityEngine.Debug.LogWarning($"[HandTracker] Desconectado — {e.Message}. Reintentando en {reconnectDelaySec}s…");
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
            ApplyHandData(frame.pointers);

        ApplySmoothing();

        if (lastHandPressed && !handPressed)
        {
            foreach (Transform hs in handSpheres)
            {
                if (hs == null) continue;
                HandDraggable d = hs.GetComponent<HandDraggable>();
                if (d != null && !d.yaColocado && d.PuedeColocarse())
                    d.Colocar();
            }
        }
        lastHandPressed = handPressed;
    }

    void ApplyHandData(PointerData[] hands)
    {
        var seen = new HashSet<int>();

        foreach (var hand in hands)
        {
            // "up" = mano levantada: limpiar inmediatamente sin grace period
            if (hand.state == "up")
            {
                RemoveHand(hand.id);
                if (hand.id == 0) handPressed = false;
                continue;
            }

            seen.Add(hand.id);

            // Asignar avatar si la mano es nueva
            if (!_manoAAvatar.TryGetValue(hand.id, out int avIdx))
            {
                avIdx = PrimerAvatarLibre();
                if (avIdx >= 0)
                {
                    _avatarEnUso[avIdx]   = true;
                    _manoAAvatar[hand.id] = avIdx;
                    _targetPos[avIdx]     = AvatarPos(hand.x, hand.y);
                    avatares[avIdx].anchoredPosition = _targetPos[avIdx];
                    avatares[avIdx].gameObject.SetActive(true);
                }
            }
            else
            {
                Vector2 next = AvatarPos(hand.x, hand.y);
                if (Vector2.Distance(_targetPos[avIdx], next) > deadZonePx)
                    _targetPos[avIdx] = next;
            }

            // screenPos sigue siendo necesario para proyectar la esfera 3D
            Vector3 screenPos = new Vector3(hand.x * Screen.width, (1f - hand.y) * Screen.height, 0f);

            // mover esfera 3D — hover-to-claim: la mano debe quedarse cerca del objeto tiempoParaReclamar segundos
            if (Camera.main != null)
            {
                Plane plane = new Plane(Vector3.up, new Vector3(0, alturaFijaY, 0));
                Ray ray = Camera.main.ScreenPointToRay(screenPos);
                if (plane.Raycast(ray, out float dist3d))
                {
                    Vector3 dest = ray.GetPoint(dist3d) + offsetVisual3D;

                    if (!handToSphere.TryGetValue(hand.id, out int sphereIdx))
                    {
                        sphereIdx = -1;
                        var claimed = new HashSet<int>(handToSphere.Values);

                        // buscar esfera no reclamada dentro del radio de reclamación
                        int nearIdx = -1;
                        float minDist = float.MaxValue;
                        for (int i = 0; i < handSpheres.Count; i++)
                        {
                            if (claimed.Contains(i) || handSpheres[i] == null) continue;
                            HandDraggable hd = handSpheres[i].GetComponent<HandDraggable>();
                            if (hd != null && hd.yaColocado) continue;
                            float d3 = Vector3.Distance(handSpheres[i].position, dest);
                            if (d3 < radioDeReclamacion && d3 < minDist) { minDist = d3; nearIdx = i; }
                        }

                        if (nearIdx >= 0)
                        {
                            handToSphere[hand.id] = nearIdx;
                            UnityEngine.Debug.Log($"[HandTracker] Mano {hand.id} reclama '{handSpheres[nearIdx].name}'");
                            sphereIdx = nearIdx;
                        }
                    }

                    if (sphereIdx >= 0 && sphereIdx < handSpheres.Count && handSpheres[sphereIdx] != null)
                    {
                        Transform hs = handSpheres[sphereIdx];
                        if (ocultarEsferaAlPerder && hs.GetComponent<HandDraggable>() == null) hs.gameObject.SetActive(true);
                        HandDraggable d = hs.GetComponent<HandDraggable>();
                        if (d == null || !d.yaColocado) sphereTarget[sphereIdx] = dest;
                    }
                }
            }

            // estado público (primera mano, compat con HandDraggable)
            if (hand.id == 0)
            {
                handPositionNormalized = new Vector2(hand.x, hand.y);
                handPressed = true;
            }
        }

        // manos perdidas por tracking (sin "up" explícito): grace period
        foreach (int key in _manoAAvatar.Keys.Except(seen).ToList())
        {
            missingFrames[key] = missingFrames.TryGetValue(key, out int f) ? f + 1 : 1;
            if (missingFrames[key] < gracePeriodFrames) continue;
            missingFrames.Remove(key);
            RemoveHand(key);
        }

        foreach (int key in seen) missingFrames.Remove(key);

        if (hands.Length == 0) handPressed = false;
    }

    void RemoveHand(int key)
    {
        if (_manoAAvatar.TryGetValue(key, out int avIdx))
        {
            avatares[avIdx].gameObject.SetActive(false);
            _avatarEnUso[avIdx] = false;
            _manoAAvatar.Remove(key);
        }
        if (!handToSphere.TryGetValue(key, out int releasedSphere)) releasedSphere = key;
        handToSphere.Remove(key);
        sphereTarget.Remove(releasedSphere);
        hoverTarget.Remove(key);
        hoverTimer.Remove(key);
        string sphereName = (releasedSphere < handSpheres.Count && handSpheres[releasedSphere] != null) ? handSpheres[releasedSphere].name : "ninguna";
        UnityEngine.Debug.Log($"[HandTracker] Mano {key} desapareció — soltó '{sphereName}'");
        if (ocultarEsferaAlPerder && releasedSphere < handSpheres.Count && handSpheres[releasedSphere] != null)
            if (handSpheres[releasedSphere].GetComponent<HandDraggable>() == null)
                handSpheres[releasedSphere].gameObject.SetActive(false);
    }

    // Interpola cada frame los avatares 2D y las esferas 3D hacia el último destino recibido por TCP.
    void ApplySmoothing()
    {
        float tAvatar = suavizadoAvatares <= 0f ? 1f : 1f - Mathf.Exp(-suavizadoAvatares * Time.deltaTime);

        if (avatares != null)
            for (int i = 0; i < avatares.Length; i++)
                if (_avatarEnUso[i] && avatares[i] != null)
                    avatares[i].anchoredPosition = Vector2.Lerp(avatares[i].anchoredPosition, _targetPos[i], tAvatar);

        foreach (var kv in sphereTarget)
        {
            int idx = kv.Key;
            if (idx < 0 || idx >= handSpheres.Count || handSpheres[idx] == null) continue;
            Transform hs = handSpheres[idx];
            HandDraggable d = hs.GetComponent<HandDraggable>();
            if (d != null && d.yaColocado) continue;
            hs.position = Vector3.Lerp(hs.position, kv.Value, tAvatar);
        }
    }

    void OnDrawGizmos()
    {
        if (handSpheres == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        foreach (Transform hs in handSpheres)
        {
            if (hs == null) continue;
            HandDraggable hd = hs.GetComponent<HandDraggable>();
            if (hd != null && hd.yaColocado) continue;
            Gizmos.DrawSphere(hs.position, radioDeReclamacion);
            Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
            Gizmos.DrawWireSphere(hs.position, radioDeReclamacion);
            Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        }
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
