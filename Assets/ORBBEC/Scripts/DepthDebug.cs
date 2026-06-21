using UnityEngine;
using OrbbecUnity;

public class DepthDebug : MonoBehaviour
{
    public OrbbecPipelineFrameSource frameSource;

    void Update()
    {
        if (frameSource == null)
        {
            Debug.Log("FrameSource NULL");
            return;
        }

        var depthFrame = frameSource.GetDepthFrame();

        if (depthFrame == null)
        {
            Debug.Log("DepthFrame NULL");
            return;
        }

        if (depthFrame.data == null)
        {
            Debug.Log("DepthFrame.data NULL");
            return;
        }

        Debug.Log(
            "Depth OK: " +
            depthFrame.width +
            " x " +
            depthFrame.height +
            " | Bytes: " +
            depthFrame.data.Length
        );
    }
}