using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;  // Скорость полета
    public float lifetime = 5f; 
    public int damage = 10;  // Урон (передадим из Player)
    private LayerMask enemyMask;  // Маска врагов
    private Rigidbody2D rb;

    private void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Bullet needs a Rigidbody2D");
        if (rb != null) rb.gravityScale = 0f; // сверху 2D — отключаем гравитацию
        Destroy(gameObject, lifetime);  // Автоуничтожение
    }

    public void SetDirection(Vector2 dir)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if ((enemyMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Bullet hit enemy with " + damage + " damage");
            }
        }
        Destroy(gameObject);
    }
}