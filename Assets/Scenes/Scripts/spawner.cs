using UnityEngine;
using System.Collections;

public class MinionSpawner : MonoBehaviour
{
    public GameObject minionPrefab;
    public Transform puntoSpawn;
    public Transform objetivoFinal;
    public float tiempoEntreOleadas = 30f;
    public int minionsPorOleada = 6;

    void Start()
    {
        StartCoroutine(SpawnOleada());
    }

    IEnumerator SpawnOleada()
    {
        while (true)
        {
            for (int i = 0; i < minionsPorOleada; i++)
            {
                GameObject minion = Instantiate(minionPrefab, puntoSpawn.position, Quaternion.identity);
                minion.GetComponent<MinionIA>().objetivoFinal = objetivoFinal;
                yield return new WaitForSeconds(0.8f);
            }
            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }
}