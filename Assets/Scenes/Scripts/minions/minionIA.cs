using UnityEngine;
using UnityEngine.AI;

public class MinionIA : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private EntityStats stats;

    public Transform objetivoFinal;
    public EntityStats objetivoActual;

    [Header("Configuración")]
    public float rangoDeteccion = 100f;
    public float rangoAtaque = 1.8f;
    public float velocidadAtaque = 1.2f;
    private float tiempoSiguienteAtaque;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();


        
        agent.stoppingDistance = rangoAtaque - agent.stoppingDistance;

        IrAMid();
    }

    void Update()
    {
        if (stats.esMuerte) return;

        // Lógica de Target
        if (objetivoActual == null || objetivoActual.esMuerte || !EstaEnRangoDeVision(objetivoActual.transform))
        {
            objetivoActual = null;
            BuscarEnemigo();
        }
        else
        {
            LógicaCombate();
        }

        ActualizarAnimaciones();
    }

    bool EstaEnRangoDeVision(Transform objetivo)
    {
        // Si el objetivo se aleja más del rango de detección, lo pierde (Leash)
        return Vector3.Distance(transform.position, objetivo.position) <= rangoDeteccion + 2f;
    }

    void BuscarEnemigo()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, rangoDeteccion);
        foreach (var hit in colliders)
        {
            EntityStats targetStats = hit.GetComponent<EntityStats>();

            // Si encuentra a alguien que no es de su equipo y está vivo
            if (targetStats != null && targetStats.equipo != stats.equipo && !targetStats.esMuerte)
            {
                // Debug.Log($"{gameObject.name} ha detectado a {hit.gameObject.name} como enemigo.");
                objetivoActual = targetStats;
                return;
            }
        }

        // Si no hay nadie, retomar camino a Mid
        IrAMid();
    }

    void IrAMid()
    {
        if (objetivoFinal != null && agent.destination != objetivoFinal.position)
        {
            agent.isStopped = false;
            agent.SetDestination(objetivoFinal.position);
        }
    }

    void LógicaCombate()
    {
        float distancia = Vector3.Distance(transform.position, objetivoActual.transform.position);

        if (distancia <= rangoAtaque)
        {
            // Parar y rotar hacia el enemigo
            agent.isStopped = true;
            RotarHaciaEnemigo(objetivoActual.transform.position);

            if (Time.time >= tiempoSiguienteAtaque)
            {
                Atacar();
                tiempoSiguienteAtaque = Time.time + velocidadAtaque;
            }
        }
        else
        {
            // Perseguir
            agent.isStopped = false;
            agent.SetDestination(objetivoActual.transform.position);
        }
    }

    void RotarHaciaEnemigo(Vector3 posicionEnemigo)
    {
        Vector3 direccion = (posicionEnemigo - transform.position).normalized;
        direccion.y = 0;
        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), Time.deltaTime * 10f);
        }
    }

    void Atacar()
    {
        anim.SetTrigger("Attack");
        // El daño se aplica aquí. En un sistema pro, se usaría un Animation Event
        objetivoActual.RecibirDaño(stats.daño);
    }

    void ActualizarAnimaciones()
    {
        // Usamos velocity.magnitude para el Blend Tree
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    // Para ver el rango en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}