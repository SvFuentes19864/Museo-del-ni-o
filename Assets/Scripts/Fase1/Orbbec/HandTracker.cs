using System.Collections.Generic;
using UnityEngine;
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
    public RectTransform orbe2D;
    public Canvas canvasPrincipal;

    [Header("Suavizado")]
    public float velocidadSuavizado = 30f;

    [HideInInspector] public Vector2 handPositionNormalized;
    [HideInInspector] public float   handDepth;
    [HideInInspector] public bool    handPressed;

    private float alturaFijaY;
    private bool  lastHandPressed;

    // static: sobrevive a desactivaciones/recargas de escena dentro de la misma sesión
    private static Process s_trackerProcess;

    private Process   trackerProcess;
    private UdpClient udpClient;
    private Thread    udpThread;
    private volatile bool running;
    private string pendingJson;
    private readonly object lockObj = new();

    private const int UDP_PORT = 7654;

    [Serializable]
    private class TrackingData
    {
        public float x;
        public float y;
        public bool  pressed;
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
        // Leer dato más reciente del hilo UDP
        string json = null;
        lock (lockObj)
        {
            json        = pendingJson;
            pendingJson = null;
        }

        if (json != null)
        {
            try
            {
                var data               = JsonUtility.FromJson<TrackingData>(json);
                handPositionNormalized = new Vector2(data.x, data.y);
                handPressed            = data.pressed;
                UnityEngine.Debug.Log($"[HandTracker] x={data.x:F3} y={data.y:F3} pressed={data.pressed}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HandTracker] Error parseando JSON '{json}': {e.Message}");
            }
        }
        else
        {
            // Solo loguea cada 120 frames para no saturar la consola
            if (Time.frameCount % 120 == 0)
                UnityEngine.Debug.LogWarning("[HandTracker] Sin datos UDP recibidos.");
        }

        // Lógica de soltar objeto al dejar de presionar
        if (lastHandPressed && !handPressed)
        {
            foreach (Transform hs in handSpheres)
            {
                if (hs == null) continue;
                HandDraggable draggable = hs.GetComponent<HandDraggable>();
                if (draggable != null && !draggable.yaColocado && draggable.PuedeColocarse())
                    draggable.Colocar();
            }
        }

        lastHandPressed = handPressed;

        // Cursor 2D en Canvas — siempre sigue la mano
        Vector3 posicionPantalla = new Vector3(
            handPositionNormalized.x * Screen.width,
            (1f - handPositionNormalized.y) * Screen.height,
            0f
        );

        if (orbe2D != null && canvasPrincipal != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasPrincipal.transform as RectTransform,
                posicionPantalla,
                canvasPrincipal.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out Vector2 posicionLocalCanvas
            );

            orbe2D.localPosition = Vector2.Lerp(
                orbe2D.localPosition,
                posicionLocalCanvas,
                velocidadSuavizado * Time.deltaTime
            );
        }

        // Cada objeto 3D sigue al cursor mientras no esté colocado
        if (Camera.main != null && orbe2D != null)
        {
            Plane planoPiso       = new Plane(Vector3.up, new Vector3(0, alturaFijaY, 0));
            Ray   rayoDesdeCamara = Camera.main.ScreenPointToRay(orbe2D.position);

            if (planoPiso.Raycast(rayoDesdeCamara, out float distancia))
            {
                Vector3 destino = rayoDesdeCamara.GetPoint(distancia) + offsetVisual3D;
                foreach (Transform hs in handSpheres)
                {
                    if (hs == null) continue;
                    HandDraggable draggable = hs.GetComponent<HandDraggable>();
                    if (draggable == null || !draggable.yaColocado)
                        hs.position = destino;
                }
            }
        }
    }

    void OnDestroy()
    {
        // Libera el socket UDP al cambiar de escena o destruir el objeto,
        // pero NO mata Python (s_trackerProcess) porque puede reutilizarse.
        StopUdpListener();
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
