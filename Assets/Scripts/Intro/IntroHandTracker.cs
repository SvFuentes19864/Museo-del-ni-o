using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Lanza el Tracking Server y recibe datos de manos por TCP (puerto 9000).
///
/// En EDITOR / desarrollo : usa pythonExecutable + pythonScriptPath del Inspector.
/// En BUILD (.exe)        : busca StreamingAssets/TrackingServer/tracking_server.exe
///                          (empaquetado con PyInstaller).
///
/// Protocolo: una línea JSON por frame →
///   {"type":"frame","frame_id":N,"pointers":[{"id":1,"x":0.5,"y":0.3,"state":"move","side":"R"}]}
/// </summary>
public class IntroHandTracker : MonoBehaviour
{
    [Header("Servidor Python")]
    public string pythonExecutable =
        @"C:\Users\alane\OneDrive\Escritorio\Tracking Server\.venv\Scripts\python.exe";
    public string pythonScriptPath =
        @"C:\Users\alane\OneDrive\Escritorio\Tracking Server\main.py";

    [Header("Conexión TCP")]
    public string serverHost       = "127.0.0.1";
    public int    serverPort       = 9000;
    public float  reconnectDelaySec = 2f;

    // Compatible con el script anterior
    [HideInInspector] public int cantidadManosDetectadas = 0;

    // Eventos — suscribirse desde otros scripts
    public event Action<PointerInfo> OnHandDown;
    public event Action<PointerInfo> OnHandMove;
    public event Action<PointerInfo> OnHandUp;

    // ── Tipos públicos ────────────────────────────────────────────────────────

    public class PointerInfo
    {
        public int    id;
        public float  x;       // [0,1] espacio proyector, 0=izquierda
        public float  y;       // [0,1] espacio proyector, 0=arriba
        public string state;   // "down" | "move" | "up"
        public string side;    // "L" | "R"
    }

    // ── Clases internas de deserialización ───────────────────────────────────

    [Serializable] private class PointerData
    {
        public int    id;
        public float  x, y;
        public string state;
        public string side;
    }

    [Serializable] private class FrameData
    {
        public string        type;
        public int           frame_id;
        public PointerData[] pointers;
    }

    // ── Proceso — static para sobrevivir cambios de escena ───────────────────

    private static Process s_serverProcess;

    // ── Estado TCP ───────────────────────────────────────────────────────────

    private TcpClient    _client;
    private Thread       _thread;
    private volatile bool _running;
    private FrameData    _pendingFrame;
    private readonly object _lock = new();

    // ── Unity lifecycle ──────────────────────────────────────────────────────

    void Start()
    {
        LaunchServer();

        _running = true;
        _thread  = new Thread(ReceiveLoop) { IsBackground = true };
        _thread.Start();

        Application.quitting += KillServer;
    }

    void Update()
    {
        FrameData frame;
        lock (_lock) { frame = _pendingFrame; _pendingFrame = null; }
        if (frame?.pointers == null) return;

        int active = 0;
        foreach (var p in frame.pointers)
        {
            if (p.state != "up") active++;

            var info = new PointerInfo
                { id = p.id, x = p.x, y = p.y, state = p.state, side = p.side };

            switch (p.state)
            {
                case "down": OnHandDown?.Invoke(info); break;
                case "move": OnHandMove?.Invoke(info); break;
                case "up":   OnHandUp?.Invoke(info);   break;
            }
        }
        cantidadManosDetectadas = active;
    }

    void OnDestroy()
    {
        _running = false;
        _client?.Close();
    }

    // ── Lanzamiento del servidor ─────────────────────────────────────────────

    void LaunchServer()
    {
        if (s_serverProcess != null && !s_serverProcess.HasExited)
        {
            UnityEngine.Debug.Log("[HandTracker] Servidor ya en ejecución — reutilizando.");
            return;
        }

        try
        {
            s_serverProcess = Process.Start(new ProcessStartInfo
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
        if (s_serverProcess == null || s_serverProcess.HasExited) return;
        try
        {
            s_serverProcess.Kill();
            s_serverProcess = null;
            UnityEngine.Debug.Log("[HandTracker] Servidor detenido.");
        }
        catch { }
    }

    // ── Hilo de recepción TCP ────────────────────────────────────────────────

    void ReceiveLoop()
    {
        while (_running)
        {
            try
            {
                _client = new TcpClient(serverHost, serverPort);
                UnityEngine.Debug.Log("[HandTracker] Conectado al servidor.");

                var stream = _client.GetStream();
                var buffer = new byte[8192];
                var sb     = new StringBuilder();

                while (_running)
                {
                    int n = stream.Read(buffer, 0, buffer.Length);
                    if (n == 0) break;

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, n));

                    string raw;
                    int idx;
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
                    UnityEngine.Debug.LogWarning(
                        $"[HandTracker] Desconectado — {e.Message}. Reintentando en {reconnectDelaySec}s…");
            }
            finally
            {
                _client?.Close();
                _client = null;
            }

            if (_running)
                Thread.Sleep((int)(reconnectDelaySec * 1000));
        }
    }
}
