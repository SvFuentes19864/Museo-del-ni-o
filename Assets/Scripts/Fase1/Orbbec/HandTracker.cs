using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Suavizado")]
    public float velocidadSuavizado = 30f;

    [HideInInspector] public Vector2 handPositionNormalized;
    [HideInInspector] public float   handDepth;
    [HideInInspector] public bool    handPressed;

    private float alturaFijaY;
    private bool  lastHandPressed;

    private readonly Dictionary<int, RectTransform> activeOrbes = new();

    private static Process s_trackerProcess;

    private Process   trackerProcess;
    private UdpClient udpClient;
    private Thread    udpThread;
    private volatile bool running;
    private string pendingJson;
    private readonly object lockObj = new();

    private const int UDP_PORT = 7654;

    [Serializable]
    private class HandData
    {
        public int   id;
        public float x;
        public float y;
        public bool  pressed;
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
            fileName  = exePath;
            arguments = "";
            workDir   = Path.GetDirectoryName(exePath);
        }
        else
        {
            string script = usarScript
                ? pythonScriptPath
                : Path.Combine(Application.streamingAssetsPath, "tracker_unity.py");

            fileName  = pythonExecutable;
            arguments = $"\"{script}\"";
            workDir   = Path.GetDirectoryName(script);
        }

        var psi = new ProcessStartInfo
        {
            CreateNoWindow   = true,
            UseShellExecute  = false,
            FileName         = fileName,
            Arguments        = arguments,
            WorkingDirectory = workDir,
        };

        try
        {
            trackerProcess   = Process.Start(psi);
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
        running   = true;
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
        }) { IsBackground = true };
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

            if (!activeOrbes.TryGetValue(hand.id, out RectTransform orbe))
            {
                if (orbe2DPrefab == null || canvasPrincipal == null) continue;
                var go = Instantiate(orbe2DPrefab, canvasPrincipal.transform);
                orbe = go.GetComponent<RectTransform>();
                var img = go.GetComponentInChildren<Image>();
                if (img != null && handColors.Length > 0)
                    img.color = handColors[hand.id % handColors.Length];
                activeOrbes[hand.id] = orbe;
            }

            // mover orbe en canvas
            Vector3 screenPos = new Vector3(hand.x * Screen.width, (1f - hand.y) * Screen.height, 0f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasPrincipal.transform as RectTransform, screenPos,
                canvasPrincipal.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out Vector2 localPos);
            orbe.localPosition = Vector2.Lerp(orbe.localPosition, localPos, velocidadSuavizado * Time.deltaTime);

            // mover esfera 3D correspondiente (sphere[id] → hand[id])
            if (Camera.main != null && hand.id < handSpheres.Count && handSpheres[hand.id] != null)
            {
                Plane plane = new Plane(Vector3.up, new Vector3(0, alturaFijaY, 0));
                Ray   ray   = Camera.main.ScreenPointToRay(orbe.position);
                if (plane.Raycast(ray, out float dist))
                {
                    Vector3 dest = ray.GetPoint(dist) + offsetVisual3D;
                    Transform hs = handSpheres[hand.id];
                    HandDraggable d = hs.GetComponent<HandDraggable>();
                    if (d == null || !d.yaColocado) hs.position = dest;
                }
            }

            // estado público (primera mano, compat con HandDraggable)
            if (hand.id == 0)
            {
                handPositionNormalized = new Vector2(hand.x, hand.y);
                handPressed = hand.pressed;
            }
        }

        // destruir orbes de manos que desaparecieron
        foreach (int key in activeOrbes.Keys.Except(seen).ToList())
        {
            Destroy(activeOrbes[key].gameObject);
            activeOrbes.Remove(key);
        }

        if (hands.Length == 0) handPressed = false;
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
        try { udpThread?.Join(300); udpThread = null; }  catch { }
    }
}
