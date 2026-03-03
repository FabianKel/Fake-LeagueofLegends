using UnityEngine;
using UnityEngine.UI;

public class EntityStats : MonoBehaviour
{
    public enum Bando { Azul, Rojo, Neutro }
    public Bando equipo;

    public float vidaMax = 30f;
    public float vidaActual;
    public float daño = 5f;
    public float deathDelay = 1.5f;
    public bool esMuerte = false;

    [Header("UI Reference")]
    public Slider barraVidaLocal;
    public Image flashRojo;

    void Start()
    {
        vidaActual = vidaMax;
        if (barraVidaLocal != null) barraVidaLocal.maxValue = vidaMax;
    }

    void Update()
    {
        if (equipo == Bando.Azul && flashRojo != null && flashRojo.color.a > 0)
        {
            var color = flashRojo.color;
            color.a -= Time.deltaTime * 2f;
            flashRojo.color = color;
        }
    }

    public void RecibirDaño(float cantidad)
    {
        if (esMuerte) return;
        vidaActual -= cantidad;

        if (barraVidaLocal != null) barraVidaLocal.value = vidaActual;

        if (equipo == Bando.Azul && flashRojo != null)
        {
            var color = flashRojo.color;
            color.a = 0.4f;
            flashRojo.color = color;
        }

        if (vidaActual <= 0) Morir();
    }

    void Morir()
    {
        esMuerte = true;
        GetComponent<Animator>().SetTrigger("Morir");
        Destroy(gameObject, deathDelay);
    }
}