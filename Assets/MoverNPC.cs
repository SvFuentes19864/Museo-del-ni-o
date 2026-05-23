using UnityEngine;

public class MoverNPC : MonoBehaviour
{
    public float velocidad = 1f;
    private float tiempo = 0f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (tiempo < 5f)
        {
            transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
            tiempo += Time.deltaTime;
        }
        else
        {
            animator.speed = 0f; // 🔥 detiene la animación
        }
    }
}