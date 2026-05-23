using UnityEngine;

public class HandTracker : MonoBehaviour
{
    public HandController hand;

    private WebCamTexture webcam => WebcamDisplay.webcam;

    void Update()
    {
        if (webcam == null || webcam.width < 100) return;

        Color[] pixels = webcam.GetPixels();

        int detectedX = 0;
        int detectedY = 0;
        int foundPixels = 0;

        for (int y = 0; y < webcam.height; y += 8)
        {
            for (int x = 0; x < webcam.width; x += 8)
            {
                int index = y * webcam.width + x;
                Color pixel = pixels[index];

                // Detectar naranja
                if (pixel.r > 0.4f &&
                pixel.g > 0.15f &&
                pixel.g < 0.5f &&
                pixel.b < 0.2f)
                {
                    detectedX += x;
                    detectedY += y;
                    foundPixels++;
                }
            }
        }

        Debug.Log("Pixels encontrados: " + foundPixels);
        if (foundPixels > 0)
        {
            detectedX /= foundPixels;
            detectedY /= foundPixels;

            float worldX = Mathf.Lerp(-5f, 5f, (float)detectedX / webcam.width);
            float worldZ = Mathf.Lerp(-5f, 5f, (float)detectedY / webcam.height);

            Vector3 targetPosition = new Vector3(worldX, 0, worldZ);

            hand.handPosition = Vector3.Lerp(
                hand.handPosition,
                targetPosition,
                Time.deltaTime * 15f
            );
        }
    }
}