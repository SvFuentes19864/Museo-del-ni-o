using UnityEngine;

public class WebcamTest : MonoBehaviour
{
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        foreach (var device in devices)
        {
            Debug.Log("Cam: " + device.name);
        }
    }
}