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
        rb.gravityScale = 0f; 
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;
    }

    public void SetSourceEnemy(GameObject enemy)
    {
        sourceEnemy = enemy;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (sourceEnemy != null && other.gameObject == sourceEnemy)
            return;

        if ((enemyMask.value & (1 << other.gameObject.layer)) > 0)
        {
            if (other.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.TakeDamage(damage);
                Debug.Log("Bullet hit enemy with " + damage + " damage");
                Destroy(gameObject);
                return;
            }
        }

        // Проверяем игрока через интерфейс IDamageable
        if (other.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            Debug.Log("Bullet hit player with " + damage + " damage");
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}