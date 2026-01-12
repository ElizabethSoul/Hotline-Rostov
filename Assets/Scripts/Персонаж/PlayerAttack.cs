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
        Vector3 origin = (firePoint != null) ? firePoint.position : transform.position;

        Vector2 dir;
        if (firePoint != null)
            dir = firePoint.right; // local forward of the fire point
        else
            dir = transform.right;

        Vector3 spawnPos = origin + (Vector3)dir.normalized * 0.1f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
        bulletInstance.damage = damage;
        bulletInstance.SetDirection(dir);

        if (debugMessages) Debug.Log($"Spawned bullet at {spawnPos} dir {dir} angle {angle}");
        Debug.DrawLine(spawnPos, spawnPos + (Vector3)dir.normalized * 0.5f, Color.blue, 0.2f);
    }

    public void IncreaseAttackSpeed(float amount)
    {
        fireCooldown = Mathf.Max(0.1f, fireCooldown - amount);
        Debug.Log($"[PlayerAttack] Скорость атаки увеличена. Новый cooldown: {fireCooldown}");
    }

    public void IncreaseBulletDamage(int amount)
    {
        damage += amount;
        Debug.Log($"[PlayerAttack] Урон пуль увеличен. Новый урон: {damage}");
    }

    // Accessors for saving/loading
    public float GetFireCooldown()
    {
        return fireCooldown;
    }

    public void SetFireCooldown(float value)
    {
        fireCooldown = Mathf.Max(0.01f, value);
    }

    public int GetDamage()
    {
        return damage;
    }

    public void SetDamage(int value)
    {
        damage = Mathf.Max(0, value);
    }
}