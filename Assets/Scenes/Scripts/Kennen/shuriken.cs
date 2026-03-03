using UnityEngine;

public class Shuriken : MonoBehaviour
{
    public float velocidad = 2000f;
    public float daño = 20f;
    private EntityStats objetivo;

    public void Setup(EntityStats target, float dañoKennen)
    {
        objetivo = target;
        daño = dañoKennen;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (objetivo == null || objetivo.esMuerte)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, objetivo.transform.position, velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, objetivo.transform.position) < 1f)
        {
            objetivo.RecibirDaño(daño);
            Destroy(gameObject);
            RedBuffIA iaRed = objetivo.GetComponent<RedBuffIA>();
            if (iaRed != null)
            {
                EntityStats statsKennen = GameObject.FindGameObjectWithTag("Player").GetComponent<EntityStats>();
                iaRed.SetAgresor(statsKennen);
            }
        }
    }
}