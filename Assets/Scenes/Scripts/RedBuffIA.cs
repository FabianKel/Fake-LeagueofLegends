using UnityEngine;
using UnityEngine.AI;

public class RedBuffIA : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private EntityStats stats;

    [Header("Configuración de Posición")]
    public Transform homePoint;
    public float distanciaLeash = 15f;
    private Vector3 SpawnPosition => homePoint != null ? homePoint.position : transform.position;

    [Header("Combate")]
    public EntityStats objetivoActual;
    public float rangoAtaque = 2.5f;
    public float velocidadAtaque = 1.5f;
    private float tiempoSiguienteAtaque;

    [Header("Buffo")]
    public float bonoDañoRed = 15f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();

        
        agent.stoppingDistance = rangoAtaque - 0.5f;
    }

    void Update()
    {
        if (stats.esMuerte) return;

        if (objetivoActual == null)
        {
            RegresarACasa();
            //Debug.Log($"{gameObject.name} no tiene objetivo, regresando a spawn.");
        }
        else
        {
            LogicaPersecucionYCombate();
        }

        ActualizarAnimaciones();
    }
    public void SetAgresor(EntityStats agresor)
    {
        if (stats.esMuerte) return;
        objetivoActual = agresor;
    }

    void LogicaPersecucionYCombate()
    {
        float distanciaAlAgresor = Vector3.Distance(transform.position, objetivoActual.transform.position);
        float distanciaAlSpawn = Vector3.Distance(transform.position, SpawnPosition);

        if (distanciaAlSpawn > distanciaLeash)
        {
            objetivoActual = null;
            return;
        }

        if (distanciaAlAgresor <= rangoAtaque)
        {
            agent.isStopped = true;
            RotarHacia(objetivoActual.transform.position);

            if (Time.time >= tiempoSiguienteAtaque)
            {
                Atacar();
                tiempoSiguienteAtaque = Time.time + velocidadAtaque;
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} persiguiendo a {objetivoActual.gameObject.name}.");
            agent.isStopped = false;
            agent.SetDestination(objetivoActual.transform.position);
        }
    }

    void RegresarACasa()
    {
        float dist = Vector3.Distance(transform.position, SpawnPosition);
        if (dist > 0.5f)
        {
            agent.isStopped = false;
            agent.SetDestination(SpawnPosition);
        }
        else
        {
            agent.isStopped = true;
            if (homePoint != null)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, homePoint.rotation, Time.deltaTime * 5f);
            }        }
    }

    void Atacar()
    {
        anim.SetTrigger("Attack");
        objetivoActual.RecibirDaño(stats.daño);
    }

    void RotarHacia(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    void ActualizarAnimaciones()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    private void OnDestroy()
    {
        if (stats != null && stats.esMuerte && objetivoActual != null)
        {
            objetivoActual.daño += bonoDañoRed;
            Debug.Log("<color=red>¡Kennen ha obtenido el BUFFO ROJO! +15 AD</color>");
        }
    }
}