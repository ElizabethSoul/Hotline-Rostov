using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;  // Скорость полета
    public float lifetime = 5f; 
    public int damage = 10; 
    private LayerMask enemyMask;  // Маска врагов
    private Rigidbody2D rb;
    private GameObject sourceEnemy;  // Враг, выстреливший пулю

    private void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Bullet needs a Rigidbody2D");
        if (rb != null) rb.gravityScale = 0f; 
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;
    }

    public void SetSourceEnemy(GameObject enemy)
    {
        sourceEnemy = enemy;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Игнорируем врага, который выстрелил
        if (sourceEnemy != null && other.gameObject == sourceEnemy)
            return;

        // Проверяем врагов
        if ((enemyMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Bullet hit enemy with " + damage + " damage");
                Destroy(gameObject);
                return;
            }
        }

        // Проверяем игрока через интерфейс IDamageable
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log("Bullet hit player with " + damage + " damage");
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}