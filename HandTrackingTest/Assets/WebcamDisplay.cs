using UnityEngine;

public class WebcamDisplay : MonoBehaviour
{
    public static WebCamTexture webcam;

    void Start()
    {
        if (webcam == null)
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            foreach (var device in devices)
            {
                if (device.name.Contains("Orbbec Femto Bolt RGB"))
                {
                    webcam = new WebCamTexture(device.name, 320, 240, 30);
                    webcam.Play();

                    Debug.Log("Usando cámara: " + device.name);
                    break;
                }
            }
        }

        Renderer renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.mainTexture = webcam;
    }
}