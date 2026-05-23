using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int piezasColocadas = 0;
    public int totalPiezas = 1;

    public void RegistrarColocacion()
    {
        piezasColocadas++;

        Debug.Log("Piezas colocadas: " + piezasColocadas);

        if (piezasColocadas >= totalPiezas)
        {
            Debug.Log("¡Fase completada!");
        }
    }
}