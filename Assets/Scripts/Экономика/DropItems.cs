using UnityEngine;

public class DropItems : MonoBehaviour
{
    [SerializeField] private GameObject walletPrefab;
    [SerializeField] private float walletDropChance = 0.7f;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float spawnCheckRadius = 0.3f;
    [SerializeField] private int maxSpawnAttempts = 10; 

    public void DropEnemyItems(Vector3 dropPosition)
    {
        if (walletPrefab == null)
        {
            Debug.LogWarning("DropEnemyItems: Не установлен префаб кошелька для выпадения предметов");
            return;
        }
        if (Random.value < walletDropChance)
        {
            InstantiateDrop(walletPrefab, dropPosition);
        }
    }
    private void InstantiateDrop(GameObject prefab, Vector3 position)
    {
        float checkRadius = spawnCheckRadius;
        Collider2D prefabCollider = prefab.GetComponent<Collider2D>();
        if (prefabCollider != null)
        {
            Vector2 ext = prefabCollider.bounds.extents;
            float approx = Mathf.Max(ext.x, ext.y);
            checkRadius = Mathf.Max(checkRadius, approx);
        }

        Vector3 spawnPos = position;
        int attempts = 0;
        while (attempts < maxSpawnAttempts)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPos = position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            Collider2D hit = Physics2D.OverlapCircle((Vector2)spawnPos, checkRadius);
            if (hit == null)
            {
                break; 
            }

            attempts++;
        }

        Object.Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
