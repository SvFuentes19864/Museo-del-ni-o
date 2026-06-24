using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class IntroHandTracker : MonoBehaviour
{
    [Header("Python Tracker")]
    public string pythonExecutable =
        @"C:\Opencv\.venv\Scripts\python.exe";

    public string pythonScriptPath =
        @"C:\Opencv\main.py";

    [HideInInspector]
    public int cantidadManosDetectadas = 0;

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

        if (json == null)
            return;

        try
        {
            var data =
                JsonUtility.FromJson
                <TrackingData>(json);

            cantidadManosDetectadas =
                data?.hands?.Length ?? 0;
        }
        catch
        {
            cantidadManosDetectadas = 0;
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