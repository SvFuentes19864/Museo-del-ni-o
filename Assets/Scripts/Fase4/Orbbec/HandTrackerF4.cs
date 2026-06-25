using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HandTrackerF4 : MonoBehaviour
{
    [Header("Python Tracker")]
    public string pythonExecutable = @"C:\Opencv\.venv\Scripts\python.exe";
    public string pythonScriptPath = @"C:\Opencv\main.py";

    [Header("Plano 3D")]
    [Tooltip("Altura Y del plano sobre el que se proyecta la mano")]
    public float alturaPlanoY = 0f;
    public Vector3 offsetVisual3DF4 = Vector3.zero;

    [Header("Cursor")]
    public GameObject orbe2DPrefabF4;
    public Canvas canvasPrincipalF4;
    public Color[] handColorsF4 =
    {
        Color.cyan, Color.yellow, Color.green, Color.magenta,
        Color.red, Color.blue, Color.white,
        new Color(1f, 0.5f, 0f), new Color(0.5f, 1f, 1f), new Color(1f, 0.5f, 1f)
    };

    // Posición 3D, posición en viewport (0-1) y estado de press de TODAS las manos.
    // HandDraggableF4 lee estos diccionarios para decidir cuál mano la controla.
    [HideInInspector] public readonly Dictionary<int, Vector3> handWorldPositions    = new();
    [HideInInspector] public readonly Dictionary<int, Vector2> handViewportPositions = new();
    [HideInInspector] public readonly Dictionary<int, bool>    handPressedStates     = new();

    private readonly Dictionary<int, RectTransform> activeOrbesF4 = new();

    private static Process s_trackerProcess;
    private Process trackerProcess;
    private UdpClient udpClient;
    private Thread udpThread;
    private volatile bool running;
    private string pendingJson;
    private readonly object lockObj = new();

    private const int UDP_PORT = 7654;

    [Serializable] private class HandData { public int id; public float x; public float y; public bool pressed; }
    [Serializable] private class TrackingData { public HandData[] hands; }

    // Llamado por ControlRuletaF4 al asignar un nuevo objeto activo.
    // Solo actualiza la altura del plano de proyección.
    public void CambiarObjetoF4(Transform nuevoObjeto)
    {
        if (nuevoObjeto != null)
            alturaPlanoY = nuevoObjeto.position.y;
    }

    void Start()
    {
        LaunchTracker();
        StartUdpListener();
    }

    void LaunchTracker()
    {
        if (s_trackerProcess != null && !s_trackerProcess.HasExited)
        {
            trackerProcess = s_trackerProcess;
            return;
        }

        string exePath = Path.Combine(Application.streamingAssetsPath, "tracker_unity", "tracker_unity.exe");
        bool usarScript = !string.IsNullOrEmpty(pythonScriptPath);

        string fileName, arguments, workDir;
        if (!usarScript && File.Exists(exePath))
        {
            fileName = exePath; arguments = ""; workDir = Path.GetDirectoryName(exePath);
        }
        else
        {
            string script = usarScript ? pythonScriptPath : Path.Combine(Application.streamingAssetsPath, "tracker_unity.py");
            fileName = pythonExecutable; arguments = $"\"{script}\""; workDir = Path.GetDirectoryName(script);
        }

        var psi = new ProcessStartInfo { CreateNoWindow = true, UseShellExecute = false, FileName = fileName, Arguments = arguments, WorkingDirectory = workDir };
        try { trackerProcess = Process.Start(psi); s_trackerProcess = trackerProcess; }
        catch (Exception e) { UnityEngine.Debug.LogError($"[HandTrackerF4] No se pudo iniciar el tracker: {e.Message}"); }
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
                if (data?.hands != null) AplicarManosF4(data.hands);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HandTrackerF4] Error parseando JSON: {e.Message}");
            }
        }
    }

    void AplicarManosF4(HandData[] hands)
    {
        var vistas = new HashSet<int>();

        foreach (var hand in hands)
        {
            vistas.Add(hand.id);

            bool isNew = !activeOrbesF4.TryGetValue(hand.id, out RectTransform orbe);
            if (isNew)
            {
                if (orbe2DPrefabF4 == null || canvasPrincipalF4 == null) continue;
                var go = Instantiate(orbe2DPrefabF4, canvasPrincipalF4.transform);
                orbe = go.GetComponent<RectTransform>();
                var img = go.GetComponent<Image>();
                if (img != null && handColorsF4.Length > 0)
                    img.color = handColorsF4[hand.id % handColorsF4.Length];
                activeOrbesF4[hand.id] = orbe;
            }

            Vector3 screenPos = new Vector3(hand.x * Screen.width, (1f - hand.y) * Screen.height, 0f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasPrincipalF4.transform as RectTransform, screenPos, null, out Vector2 localPos);
            orbe.localPosition = localPos;

            // calcular posición 3D y viewport para TODAS las manos y exponerlas
            handPressedStates[hand.id]     = hand.pressed;
            handViewportPositions[hand.id] = new Vector2(hand.x, 1f - hand.y); // Y invertida: tracker=top-down, viewport=bottom-up
            if (Camera.main != null)
            {
                Plane plane = new Plane(Vector3.up, new Vector3(0, alturaPlanoY, 0));
                Ray ray = Camera.main.ScreenPointToRay(orbe.position);
                if (plane.Raycast(ray, out float dist))
                    handWorldPositions[hand.id] = ray.GetPoint(dist) + offsetVisual3DF4;
            }
        }

        // limpiar manos que desaparecieron
        foreach (int key in new List<int>(activeOrbesF4.Keys))
        {
            if (vistas.Contains(key)) continue;
            Destroy(activeOrbesF4[key].gameObject);
            activeOrbesF4.Remove(key);
            handWorldPositions.Remove(key);
            handViewportPositions.Remove(key);
            handPressedStates.Remove(key);
        }
    }

    void OnDestroy()
    {
        running = false;
        try { udpClient?.Close(); udpClient = null; } catch { }
        try { udpThread?.Join(300); udpThread = null; } catch { }
        foreach (var orbe in activeOrbesF4.Values)
            if (orbe != null) Destroy(orbe.gameObject);
        activeOrbesF4.Clear();
    }

    void OnApplicationQuit()
    {
        running = false;
        try { udpClient?.Close(); } catch { }
        if (s_trackerProcess != null && !s_trackerProcess.HasExited)
        {
            s_trackerProcess.Kill();
            s_trackerProcess.Dispose();
            s_trackerProcess = null;
        }
    }
}
