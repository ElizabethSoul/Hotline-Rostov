using UnityEngine;

public class DropItems : MonoBehaviour
{
    [SerializeField] private GameObject walletPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private float walletDropChance = 0.7f;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float spawnCheckRadius = 0.3f; // минимальная дистанция между предметами
    [SerializeField] private int maxSpawnAttempts = 10; // сколько раз пробуем найти свободную точку

    public void DropEnemyItems(Vector3 dropPosition)
    {
        if (walletPrefab == null || weaponPrefab == null)
        {
            Debug.LogWarning("DropEnemyItems: Не установлены префабы для выпадения предметов");
            return;
        }

        // 70% вероятность выпадения кошелька
        if (Random.value < walletDropChance)
        {
            InstantiateDrop(walletPrefab, dropPosition);
        }

        // 100% выпадение оружия
        InstantiateDrop(weaponPrefab, dropPosition);
    }
    public void DropEnemyItems(Transform weaponObject, Vector3 dropPosition)
    {
        if (walletPrefab == null || weaponObject == null)
        {
            Debug.LogWarning("DropEnemyItems: Не установлены префабы для выпадения предметов");
            return;
        }

        // 70% вероятность выпадения кошелька
        if (Random.value < walletDropChance)
        {
            InstantiateDrop(walletPrefab, dropPosition);
        }

        // 100% выпадение оружия (дочерний объект)
        if (weaponObject != null)
        {
            InstantiateDrop(weaponObject.gameObject, dropPosition);
        }
    }

    private void InstantiateDrop(GameObject prefab, Vector3 position)
    {
        // Определяем радиус проверки по коллайдеру префаба (если есть)
        float checkRadius = spawnCheckRadius;
        Collider2D prefabCollider = prefab.GetComponent<Collider2D>();
        if (prefabCollider != null)
        {
            // берем максимум по осям, чтобы учесть размер предмета
            Vector2 ext = prefabCollider.bounds.extents;
            float approx = Mathf.Max(ext.x, ext.y);
            checkRadius = Mathf.Max(checkRadius, approx);
        }

        Vector3 spawnPos = position;
        int attempts = 0;
        // Пробуем несколько случайных позиций внутри радиуса, пока не найдем свободную
        while (attempts < maxSpawnAttempts)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPos = position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            Collider2D hit = Physics2D.OverlapCircle((Vector2)spawnPos, checkRadius);
            if (hit == null)
            {
                break; // место свободно
            }

            attempts++;
        }

        Object.Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
