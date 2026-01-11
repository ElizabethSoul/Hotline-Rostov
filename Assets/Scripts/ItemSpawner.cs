using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;
    public Transform[] itemSpawnPoints;
    public float timeBetweenWaves = 30f;
    public int itemsPerWave = 3;
    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (true)
        {   
            for (int j = 0; j < itemsPerWave; j++)
            {
                Transform itemSpawn = itemSpawnPoints[Random.Range(0, itemSpawnPoints.Length)];
                GameObject item;
                float chance = Random.value;
                item = chance < 0.5f ? itemPrefabs[0] : itemPrefabs[1]; // 50/50 при нормальном HP
                Instantiate(item, itemSpawn.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}
