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
    public string pythonExecutable =
        @"C:\Opencv\.venv\Scripts\python.exe";

    public string pythonScriptPath =
        @"C:\Opencv\main.py";

    [Header("Objeto actual F4")]
    public Transform objetoActualF4;

    public Vector3 offsetVisual3DF4 =
        Vector3.zero;

    [Header("Cursor")]
    public GameObject orbe2DPrefabF4;

    public Canvas canvasPrincipalF4;

    public Color[] handColorsF4 =
    {
        Color.cyan,
        Color.yellow,
        Color.green,
        Color.magenta,
        Color.red,
        Color.blue,
        Color.white,
        new Color(1f,0.5f,0f),
        new Color(0.5f,1f,1f),
        new Color(1f,0.5f,1f)
    };

    private readonly Dictionary<int, RectTransform>
        activeOrbesF4 = new();

    [Header("Suavizado")]
    public float velocidadSuavizadoF4 = 30f;

    [HideInInspector]
    public bool handPressedF4;

    private float alturaFijaYF4;

    private bool lastHandPressedF4;

    private static Process s_trackerProcess;

    private Process trackerProcess;
    private UdpClient udpClient;
    private Thread udpThread;

    private volatile bool running;

    private string pendingJson;

    private readonly object lockObj =
        new object();

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

    public void CambiarObjetoF4(
        Transform nuevoObjeto
    )
    {
        objetoActualF4 = nuevoObjeto;

        if (objetoActualF4 != null)
        {
            alturaFijaYF4 =
                objetoActualF4.position.y;
        }
    }

    void Start()
    {
        LaunchTracker();
        StartUdpListener();
    }

    void LaunchTracker()
    {
        if (
            s_trackerProcess != null &&
            !s_trackerProcess.HasExited
        )
        {
            trackerProcess =
                s_trackerProcess;

            return;
        }

        string exePath =
            Path.Combine(
                Application.streamingAssetsPath,
                "tracker_unity",
                "tracker_unity.exe"
            );

        string fileName;
        string arguments;
        string workDir;

        bool usarScript =
            !string.IsNullOrEmpty(
                pythonScriptPath
            );

        if (
            !usarScript &&
            File.Exists(exePath)
        )
        {
            fileName = exePath;
            arguments = "";
            workDir =
                Path.GetDirectoryName(
                    exePath
                );
        }
        else
        {
            string script =
                pythonScriptPath;

            fileName =
                pythonExecutable;

            arguments =
                $"\"{script}\"";

            workDir =
                Path.GetDirectoryName(
                    script
                );
        }

        var psi =
            new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workDir
            };

        try
        {
            trackerProcess =
                Process.Start(psi);

            s_trackerProcess =
                trackerProcess;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(
                e.Message
            );
        }
    }

    void StartUdpListener()
    {
        running = true;

        udpClient =
            new UdpClient(
                UDP_PORT
            );

        udpThread =
            new Thread(() =>
            {
                IPEndPoint ep =
                    new(
                        IPAddress.Any,
                        0
                    );

                while (running)
                {
                    try
                    {
                        byte[] data =
                            udpClient.Receive(
                                ref ep
                            );

                        string json =
                            Encoding.UTF8
                                .GetString(
                                    data
                                );

                        lock (lockObj)
                        {
                            pendingJson =
                                json;
                        }
                    }
                    catch { }
                }
            });

        udpThread.IsBackground =
            true;

        udpThread.Start();
    }

    void Update()
    {
        string json = null;

        lock (lockObj)
        {
            json = pendingJson;
            pendingJson = null;
        }

        if (json != null)
        {
            var data =
                JsonUtility
                    .FromJson
                    <TrackingData>(json);

            if (
                data != null &&
                data.hands != null
            )
            {
                AplicarManosF4(
                    data.hands
                );
            }
        }
    }

    void AplicarManosF4(
        HandData[] hands
    )
    {
        HashSet<int> vistas =
            new HashSet<int>();

        foreach (var hand in hands)
        {
            vistas.Add(hand.id);

            if (
                !activeOrbesF4.TryGetValue(
                    hand.id,
                    out RectTransform orbe
                )
            )
            {
                GameObject go =
                    Instantiate(
                        orbe2DPrefabF4,
                        canvasPrincipalF4.transform
                    );

                orbe =
                    go.GetComponent<RectTransform>();

                Image img =
                    go.GetComponent<Image>();

                if (
                    img != null &&
                    handColorsF4.Length > 0
                )
                {
                    img.color =
                        handColorsF4[
                            hand.id %
                            handColorsF4.Length
                        ];
                }

                activeOrbesF4.Add(
                    hand.id,
                    orbe
                );
            }

            Vector3 screenPos =
                new Vector3(
                    hand.x * Screen.width,
                    (1f - hand.y) * Screen.height,
                    0f
                );

            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasPrincipalF4.transform
                        as RectTransform,
                    screenPos,
                    null,
                    out Vector2 localPos
                );

            orbe.localPosition =
                Vector2.Lerp(
                    orbe.localPosition,
                    localPos,
                    velocidadSuavizadoF4 *
                    Time.deltaTime
                );
        }

        List<int> borrar =
            new List<int>();

        foreach (
            var kvp
            in activeOrbesF4
        )
        {
            if (!vistas.Contains(kvp.Key))
            {
                Destroy(
                    kvp.Value.gameObject
                );

                borrar.Add(kvp.Key);
            }
        }

        foreach (int id in borrar)
        {
            activeOrbesF4.Remove(id);
        }

        foreach (var hand in hands)
        {
            if (hand.id == 0)
            {
                AplicarManoF4(hand);
                break;
            }
        }
    }

    void AplicarManoF4(
        HandData hand
    )
    {
        handPressedF4 =
            hand.pressed;

        Vector3 screenPos =
            new Vector3(
                hand.x *
                Screen.width,

                (1f - hand.y) *
                Screen.height,

                0f
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasPrincipalF4
                    .transform
                    as RectTransform,

                screenPos,

                null,

                out Vector2 localPos
            );

        if (
            activeOrbesF4.TryGetValue(
                hand.id,
                out RectTransform orbe
            )
        )
        {
            orbe.localPosition =
                Vector2.Lerp(
                    orbe.localPosition,
                    localPos,
                    velocidadSuavizadoF4 *
                    Time.deltaTime
                );
        }

        HandDraggableF4 draggable =
            null;

        if (objetoActualF4 != null)
        {
            draggable =
                objetoActualF4
                    .GetComponent
                    <HandDraggableF4>();
        }

        if (
            lastHandPressedF4 &&
            !handPressedF4
        )
        {
            if (
                draggable != null &&
                !draggable.yaColocadoF4
            )
            {
                if (
                    draggable
                        .PuedeColocarseF4()
                )
                {
                    draggable
                        .ColocarF4();
                }
            }
        }

        lastHandPressedF4 =
            handPressedF4;

        if (
            objetoActualF4 != null &&
            Camera.main != null &&
            activeOrbesF4.TryGetValue(
                0,
                out RectTransform cursorPrincipal
            )
        )
        {
            if (
                draggable == null ||
                !draggable.yaColocadoF4
            )
            {
                Plane plane =
                    new Plane(
                        Vector3.up,
                        new Vector3(
                            0,
                            alturaFijaYF4,
                            0
                        )
                    );

                Ray ray =
                    Camera.main
                        .ScreenPointToRay(
                            cursorPrincipal.position
                        );

                if (
                    plane.Raycast(
                        ray,
                        out float dist
                    )
                )
                {
                    objetoActualF4.position =
                        ray.GetPoint(
                            dist
                        ) +
                        offsetVisual3DF4;
                }
            }
        }
    }

    void OnDestroy()
    {
        running = false;

        try
        {
            udpClient?.Close();
        }
        catch { }
    }
}