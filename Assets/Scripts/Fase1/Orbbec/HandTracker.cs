using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HandTracker : MonoBehaviour
{
    [Header("Python Tracker")]
    [Tooltip("Python del venv, ej: C:\\Opencv\\.venv\\Scripts\\python.exe")]
    public string pythonExecutable = @"C:\Opencv\.venv\Scripts\python.exe";
    [Tooltip("Ruta al script main.py. Vacío = usa StreamingAssets/tracker_unity.py")]
    public string pythonScriptPath = @"C:\Opencv\main.py";

    [Header("Objetos 3D a mover")]
    public List<Transform> handSpheres = new();
    [Tooltip("Usa esto si los objetos necesitan un pequeño empujón en X o Z para verse centrados debajo del cursor")]
    public Vector3 offsetVisual3D = Vector3.zero;

    [Header("Interacción 2D (Canvas)")]
    public GameObject orbe2DPrefab;
    public Canvas canvasPrincipal;
    public Color[] handColors = { Color.cyan, Color.yellow, Color.green, Color.magenta, Color.red, Color.blue };

    [Header("Reclamación de objeto")]
    [Tooltip("Radio (metros) dentro del cual la mano puede reclamar un objeto")]
    public float radioDeReclamacion = 1f;
    [Tooltip("Segundos que la mano debe estar dentro del radio antes de poder mover el objeto")]
    public float tiempoParaReclamar = 1f;

    [Header("Suavizado de movimiento")]
    [Tooltip("Qué tan rápido el orbe/esfera alcanza la posición recibida. Mayor = más responsivo (sigue de cerca, casi sin delay), menor = más suave. 0 = directo, sin delay (confía en el suavizado de Python). Si sientes delay al arrastrar, súbelo o ponlo en 0")]
    public float suavizadoVelocidad = 35f;

    [HideInInspector] public Vector2 handPositionNormalized;
    [HideInInspector] public float handDepth;
    [HideInInspector] public bool handPressed;

    private float alturaFijaY;
    private bool lastHandPressed;

    private readonly Dictionary<int, RectTransform> activeOrbes = new();
    private readonly Dictionary<int, int>   handToSphere   = new(); // hand.id → índice en handSpheres (reclamado)
    private readonly Dictionary<int, int>   hoverTarget    = new(); // hand.id → esfera bajo hover
    private readonly Dictionary<int, float> hoverTimer     = new(); // hand.id → segundos acumulados
    private readonly Dictionary<int, int>   missingFrames  = new(); // hand.id → frames consecutivos sin detectar
    private readonly Dictionary<int, Vector2> orbeTarget   = new(); // hand.id → target localPos del orbe 2D
    private readonly Dictionary<int, Vector3> sphereTarget = new(); // sphereIdx → target world pos de la esfera 3D

    [Header("Estabilidad de tracking")]
    [Tooltip("Frames consecutivos sin detectar la mano antes de considerarla desaparecida")]
    public int gracePeriodFrames = 5;
    [Tooltip("Ocultar la esfera 3D cuando la mano desaparece. Desactivar si la esfera es un objeto del juego (no solo cursor)")]
    public bool ocultarEsferaAlPerder = true;

    private static Process s_trackerProcess;

    private Process trackerProcess;
    private UdpClient udpClient;
    private Thread udpThread;
    private volatile bool running;
    private string pendingJson;
    private readonly object lockObj = new();

    private const int UDP_PORT = 7654;

    [Serializable]
    private class HandData
    {
        public int id;
        public float x;
        public float y;
        public bool pressed;
    }

    [Serializable]
    private class TrackingData
    {
        public HandData[] hands;
    }

    void Start()
    {
        if (handSpheres.Count > 0 && handSpheres[0] != null)
            alturaFijaY = handSpheres[0].position.y;

        LaunchTracker();
        StartUdpListener();
        UnityEngine.Debug.Log($"[HandTracker] LISTO en escena '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' — esferas: {handSpheres.Count}, radioReclamacion: {radioDeReclamacion}");
    }

    void LaunchTracker()
    {
        // Si ya hay un proceso activo (de esta u otra instancia), reutilizarlo
        if (s_trackerProcess != null && !s_trackerProcess.HasExited)
        {
            trackerProcess = s_trackerProcess;
            UnityEngine.Debug.Log("[HandTracker] Tracker ya en ejecución, reutilizando proceso existente.");
            return;
        }

        string exePath = Path.Combine(Application.streamingAssetsPath, "tracker_unity", "tracker_unity.exe");

        string fileName, arguments, workDir;

        // Si pythonScriptPath está definido, forzar modo script (ignora el exe)
        bool usarScript = !string.IsNullOrEmpty(pythonScriptPath);

        if (!usarScript && File.Exists(exePath))
        {
            fileName = exePath;
            arguments = "";
            workDir = Path.GetDirectoryName(exePath);
        }
        else
        {
            string script = usarScript
                ? pythonScriptPath
                : Path.Combine(Application.streamingAssetsPath, "tracker_unity.py");

            fileName = pythonExecutable;
            arguments = $"\"{script}\"";
            workDir = Path.GetDirectoryName(script);
        }

        var psi = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workDir,
        };

        try
        {
            trackerProcess = Process.Start(psi);
            s_trackerProcess = trackerProcess;   // guardar referencia estática
            UnityEngine.Debug.Log("[HandTracker] Tracker iniciado.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[HandTracker] No se pudo iniciar el tracker: {e.Message}");
        }
    }

    void StartUdpListener()
    {
        running = true;
        udpClient = new UdpClient(UDP_PORT);

        udpThread = new Thread(() =>
        {
            IPEndPoint ep = new(IPAddress.Any, 0);
            while (running)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref ep);
                    string json = Encoding.UTF8.GetString(data);
                    lock (lockObj) { pendingJson = json; }
                }
                catch { }
            }
        })
        { IsBackground = true };
        udpThread.Start();
    }

    void Update()
    {
        string json = null;
        lock (lockObj) { json = pendingJson; pendingJson = null; }

        if (json != null)
        {
            try
            {
                var data = JsonUtility.FromJson<TrackingData>(json);
                ApplyHandData(data.hands ?? Array.Empty<HandData>());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HandTracker] Error parseando JSON '{json}': {e.Message}");
            }
        }

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

    void ApplyHandData(HandData[] hands)
    {
        var seen = new HashSet<int>();

        foreach (var hand in hands)
        {
            seen.Add(hand.id);

            bool isNew = !activeOrbes.TryGetValue(hand.id, out RectTransform orbe);
            if (isNew)
            {
                if (orbe2DPrefab == null || canvasPrincipal == null) continue;
                var go = Instantiate(orbe2DPrefab, canvasPrincipal.transform);
                orbe = go.GetComponent<RectTransform>();
                var img = go.GetComponentInChildren<Image>();
                if (img != null && handColors.Length > 0)
                    img.color = handColors[hand.id % handColors.Length];
                go.GetComponentInChildren<TextMeshProUGUI>()?.SetText(hand.id.ToString());
                activeOrbes[hand.id] = orbe;
            }

            // mover orbe en canvas (se guarda el destino; la interpolación ocurre en ApplySmoothing)
            Vector3 screenPos = new Vector3(hand.x * Screen.width, (1f - hand.y) * Screen.height, 0f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasPrincipal.transform as RectTransform, screenPos,
                canvasPrincipal.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out Vector2 localPos);
            orbeTarget[hand.id] = localPos;
            if (isNew) orbe.localPosition = localPos; // primer frame sin interpolar (evita que "vuele" desde el origen)

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
                handPressed = hand.pressed;
            }
        }

        // destruir orbes de manos que desaparecieron (con grace period)
        foreach (int key in activeOrbes.Keys.Except(seen).ToList())
        {
            missingFrames[key] = missingFrames.TryGetValue(key, out int f) ? f + 1 : 1;
            if (missingFrames[key] < gracePeriodFrames) continue; // todavía en gracia, no eliminar

            missingFrames.Remove(key);
            Destroy(activeOrbes[key].gameObject);
            activeOrbes.Remove(key);
            orbeTarget.Remove(key);
            if (!handToSphere.TryGetValue(key, out int releasedSphere)) releasedSphere = key;
            handToSphere.Remove(key);
            sphereTarget.Remove(releasedSphere);
            hoverTarget.Remove(key);
            hoverTimer.Remove(key);
            string sphereName = (releasedSphere < handSpheres.Count && handSpheres[releasedSphere] != null) ? handSpheres[releasedSphere].name : "ninguna";
            UnityEngine.Debug.Log($"[HandTracker] Mano {key} desapareció — soltó '{sphereName}'");
            if (ocultarEsferaAlPerder && releasedSphere < handSpheres.Count && handSpheres[releasedSphere] != null)
            {
                if (handSpheres[releasedSphere].GetComponent<HandDraggable>() == null)
                    handSpheres[releasedSphere].gameObject.SetActive(false);
            }
        }

        // resetear grace period para manos que volvieron a aparecer
        foreach (int key in seen)
            missingFrames.Remove(key);

        if (hands.Length == 0) handPressed = false;
    }

    // Interpola cada frame el orbe 2D y la esfera 3D hacia el último destino recibido por UDP.
    // Como Python envía a menor FPS que Unity, esto evita los "saltos" entre paquetes y hace
    // que el movimiento se sienta fluido. El factor 1-exp(-k*dt) es independiente del framerate.
    void ApplySmoothing()
    {
        float t = suavizadoVelocidad <= 0f ? 1f : 1f - Mathf.Exp(-suavizadoVelocidad * Time.deltaTime);

        foreach (var kv in orbeTarget)
            if (activeOrbes.TryGetValue(kv.Key, out RectTransform rt) && rt != null)
                rt.localPosition = Vector2.Lerp(rt.localPosition, kv.Value, t);

        foreach (var kv in sphereTarget)
        {
            int idx = kv.Key;
            if (idx < 0 || idx >= handSpheres.Count || handSpheres[idx] == null) continue;
            Transform hs = handSpheres[idx];
            HandDraggable d = hs.GetComponent<HandDraggable>();
            if (d != null && d.yaColocado) continue;
            hs.position = Vector3.Lerp(hs.position, kv.Value, t);
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

    void OnDestroy()
    {
        StopUdpListener();
        foreach (var orbe in activeOrbes.Values)
            if (orbe != null) Destroy(orbe.gameObject);
        activeOrbes.Clear();
    }

    void OnApplicationQuit()
    {
        StopUdpListener();

        if (s_trackerProcess != null && !s_trackerProcess.HasExited)
        {
            s_trackerProcess.Kill();
            s_trackerProcess.Dispose();
            s_trackerProcess = null;
        }
        trackerProcess = null;
    }

    void StopUdpListener()
    {
        running = false;
        try { udpClient?.Close(); udpClient = null; } catch { }
        try { udpThread?.Join(300); udpThread = null; } catch { }
    }
}
