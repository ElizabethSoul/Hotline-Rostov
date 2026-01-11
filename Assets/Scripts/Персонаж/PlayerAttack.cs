using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bullet bulletPrefab;         // назначайте префаб Bullet в инспекторе
    [SerializeField] private Transform firePoint;         // точка появления пули (опционально)

    [Header("Shooting settings")]
    [SerializeField] public int damage = 10;
    [SerializeField] private float fireCooldown = 0.5f;
    [SerializeField] private bool debugMessages = false;

    private float nextFireTime;

    // Внешний вызов для стрельбы (оставлено простым — без state machine и ударов)

    public void OnAttack(){
        StartAttack();
    }
    public void StartAttack()
    {
        if (Time.time < nextFireTime) return;
        SpawnBullet();
        nextFireTime = Time.time + fireCooldown;
    }

    private void SpawnBullet()
    {
        Vector3 origin = firePoint.position;
        Vector2 dir = firePoint.position - transform.position;
        Vector3 spawnPos = origin + (Vector3)dir*0.5f;

        var bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bulletInstance.damage = damage;
        bulletInstance.SetDirection(dir);

        if (debugMessages) Debug.Log($"Spawned bullet at {spawnPos} dir {dir}");
        Debug.DrawLine(spawnPos, spawnPos + Vector3.up * 0.1f, Color.blue, 0.2f);
    }
}