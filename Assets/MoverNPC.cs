using UnityEngine;
using UnityEngine.AI;

public class MoverNPC : MonoBehaviour
{
    public float rangoMovimiento = 3f;
    public float tiempoEsperaMin = 4f;
    public float tiempoEsperaMax = 10f;

    private NavMeshAgent agent;
    private float temporizador;
    private float siguienteCambio;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Tiempo diferente para cada NPC
        siguienteCambio = Random.Range(tiempoEsperaMin, tiempoEsperaMax);

        // Offset inicial random
        temporizador = Random.Range(0f, siguienteCambio);

        if (agent.isOnNavMesh)
        {
            ElegirNuevoDestino();
        }
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
            return;

        temporizador += Time.deltaTime;

        if (
            temporizador >= siguienteCambio ||
            (!agent.pathPending &&
             agent.remainingDistance <= agent.stoppingDistance)
        )
        {
            ElegirNuevoDestino();
        }
    }

    void ElegirNuevoDestino()
    {
        temporizador = 0f;

        // Nuevo tiempo random cada vez
        siguienteCambio = Random.Range(tiempoEsperaMin, tiempoEsperaMax);

        Vector3 randomDirection = Random.insideUnitSphere * rangoMovimiento;
        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection,
            out hit,
            rangoMovimiento,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}