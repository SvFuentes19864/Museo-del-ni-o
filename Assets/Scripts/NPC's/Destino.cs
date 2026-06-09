using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public UnityEngine.AI.NavMeshAgent AI;
    public float Velocidad;
    public Transform[] Objetivos;
    Transform Objetivo;
    float Distancia;

    [Header("Animaciones")]
    public Animation Anim;
    public string CaminandoAnim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Objetivo = Objetivos[Random.Range(0, Objetivos.Length)];

        Anim.Play(CaminandoAnim);
    }

    // Update is called once per frame
    void Update()
    {
        Distancia = Vector3.Distance(transform.position, Objetivo.position);

        if (Distancia > 1)
        {
            Objetivo = Objetivos[Random.Range(0, Objetivos.Length)];
        }

        AI.destination = Objetivo.position;
        AI.speed = Velocidad;
    }
}
