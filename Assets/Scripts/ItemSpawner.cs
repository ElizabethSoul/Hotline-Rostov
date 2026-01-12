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
                if (itemSpawnPoints == null || itemSpawnPoints.Length == 0)
                    yield break;

                Transform itemSpawn = itemSpawnPoints[Random.Range(0, itemSpawnPoints.Length)];

                if (itemPrefabs == null || itemPrefabs.Length == 0)
                    continue;

                GameObject item;
                if (itemPrefabs.Length == 1)
                {
                    item = itemPrefabs[0];
                }
                else
                {
                    item = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
                }

                Instantiate(item, itemSpawn.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}
