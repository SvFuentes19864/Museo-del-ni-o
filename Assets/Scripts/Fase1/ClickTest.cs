using UnityEngine;

public class ClickTest : MonoBehaviour
{
    public void ClickRecibido()
    {
        Debug.Log(
            "CLICK RECIBIDO EN: " +
            gameObject.name
        );
    }
}