using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ManejadorMovimiento : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    [Header("Configuración")]
    public LayerMask groundLayer;
    public LayerMask entityLayer;
    public float suavizadoAnimacion = 0.05f;
    public float velocidadAtaque = 1.0f;

    [Header("UI Target")]
    public GameObject panelTarget;
    public Slider barraTarget;
    public TMP_Text nombreTarget;

    [Header("Cursores")]
    public Texture2D cursorNormal;
    public Texture2D cursorAtaque;

    [Header("Ataque a Distancia")]
    public GameObject shurikenPrefab;
    public Transform puntoDisparo;
    public float rangoAtaque = 50f;

    private EntityStats targetActual;
    private EntityStats enemigoAAtacar;
    private float tiempoSiguienteAtaque;

    private LineRenderer lineRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.updateRotation = false;
        agent.acceleration = 1000f;
        Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    void Update()
    {
        DetectarMouseSobreEnemigo();

        // Click Derecho: Mover o Atacar
        if (Input.GetMouseButtonDown(1))
        {
            Interactuar();
        }

        // Click Izquierdo: Seleccionar para ver vida
        if (Input.GetMouseButtonDown(0))
        {
            SeleccionarTarget();
        }

        ProcesarAtaqueContinuo();

        ActualizarAnimaciones();
        ProcesarRotacion();
        ActualizarUITarget();
    }


    void ProcesarAtaqueContinuo()
    {
        if (enemigoAAtacar == null) return;

        if (enemigoAAtacar.esMuerte)
        {
            enemigoAAtacar = null;
            return;
        }

        float distancia = Vector3.Distance(transform.position, enemigoAAtacar.transform.position);
        

        if (distancia > rangoAtaque)
        {
            agent.SetDestination(enemigoAAtacar.transform.position);
        }
        else
        {
            agent.ResetPath();

            Vector3 dir = (enemigoAAtacar.transform.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero) transform.forward = dir;

            if (Time.time >= tiempoSiguienteAtaque)
            {
                EjecutarAtaque(enemigoAAtacar);
                tiempoSiguienteAtaque = Time.time + velocidadAtaque;
            }
        }
    }

    void DetectarMouseSobreEnemigo()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, entityLayer))
        {
            EntityStats stats = hit.collider.GetComponent<EntityStats>();
            if (stats != null && (stats.equipo == EntityStats.Bando.Rojo || stats.equipo == EntityStats.Bando.Neutro ))
            {
                Cursor.SetCursor(cursorAtaque, Vector2.zero, CursorMode.Auto);
                return;
            }
        }
        Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
    }

    void Interactuar()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, entityLayer))
        {
            EntityStats statsEnemigo = hit.collider.GetComponent<EntityStats>();
            if (statsEnemigo != null && (statsEnemigo.equipo == EntityStats.Bando.Rojo || statsEnemigo.equipo == EntityStats.Bando.Neutro))
            {
                enemigoAAtacar = statsEnemigo;

                return;
            }
        }

        enemigoAAtacar = null;
        MoverA_Click();
    }

    void EjecutarAtaque(EntityStats enemigo)
    {
        Vector3 dir = (enemigo.transform.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.forward = dir;

        anim.SetTrigger("Attack");

        GameObject proj = Instantiate(shurikenPrefab, puntoDisparo.position, Quaternion.identity);
        proj.GetComponent<Shuriken>().Setup(enemigo, GetComponent<EntityStats>().daño);

        MostrarTargetEnUI(enemigo);
    }

    void SeleccionarTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, entityLayer))
        {
            EntityStats stats = hit.collider.GetComponent<EntityStats>();
            if (stats != null) MostrarTargetEnUI(stats);
        }
        else
        {
            panelTarget.SetActive(false);
            targetActual = null;
        }
    }

    void MostrarTargetEnUI(EntityStats stats)
    {
        targetActual = stats;
        panelTarget.SetActive(true);
        barraTarget.maxValue = stats.vidaMax;
        nombreTarget.text = stats.gameObject.name.Replace("(Clone)", "");
    }

    void ActualizarUITarget()
    {
        if (targetActual != null)
        {
            barraTarget.value = targetActual.vidaActual;
            if (targetActual.esMuerte) panelTarget.SetActive(false);
        }
    }

    void MoverA_Click()

    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            agent.ResetPath();

            agent.SetDestination(hit.point);
            Vector3 direccion = (hit.point - transform.position).normalized;
            if (direccion != Vector3.zero)
            {
                direccion.y = 0;
                transform.forward = direccion;
            }
        }
    }

    void ProcesarRotacion()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direccionMovimiento = agent.velocity.normalized;
            direccionMovimiento.y = 0;
            transform.rotation = Quaternion.LookRotation(direccionMovimiento);
        }
    }
    void ActualizarAnimaciones()
    {
        float velocidadActual = agent.velocity.magnitude;
        anim.SetFloat("Speed", velocidadActual, suavizadoAnimacion, Time.deltaTime);
    }
}