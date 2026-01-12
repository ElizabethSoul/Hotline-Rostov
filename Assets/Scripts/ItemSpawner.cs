using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject[] itemPrefabs;
    public Transform[] itemSpawnPoints;
    
    public float timeBetweenWaves = 30f;
    public int itemsPerWave = 3;
    
    [Header("Проверка занятости")]
    [SerializeField] private float checkRadius = 0.4f;      // подгоните под размер ваших спрайтов
    [SerializeField] private string itemTag = "Item";

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (true)
        {
            // Собираем список свободных точек
            List<Transform> freePoints = GetFreeSpawnPoints();

            // Сколько реально можем заспавнить
            int toSpawn = Mathf.Min(itemsPerWave, freePoints.Count);

            // Если нет свободных точек — пропускаем волну
            if (toSpawn == 0)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
                continue;
            }

            // Спавним нужное количество предметов
            for (int i = 0; i < toSpawn; i++)
            {
                // Выбираем случайную свободную точку
                int index = Random.Range(0, freePoints.Count);
                Transform spawnPoint = freePoints[index];

                // Удаляем точку из списка, чтобы не использовать повторно в этой волне
                freePoints.RemoveAt(index);

                SpawnRandomItem(spawnPoint);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private List<Transform> GetFreeSpawnPoints()
    {
        List<Transform> free = new List<Transform>();

        foreach (var point in itemSpawnPoints)
        {
            if (point == null) continue;

            // Проверяем, есть ли рядом объекты с тегом "Item"
            Collider2D[] hits = Physics2D.OverlapCircleAll(point.position, checkRadius);

            bool isOccupied = false;
            foreach (var hit in hits)
            {
                if (hit.CompareTag(itemTag))
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                free.Add(point);
            }
        }

        return free;
    }

    private void SpawnRandomItem(Transform spawnPoint)
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        GameObject prefabToSpawn = itemPrefabs.Length == 1 
            ? itemPrefabs[0] 
            : itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
    }
}