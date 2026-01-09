using UnityEngine;

public class RangeAttackState : EnemyState
{
    private float lastAttackTime;

    public RangeAttackState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
        lastAttackTime = -999f;
    }

    public override void Enter()
    {
        enemy.Stop();
    }

    public override void LogicUpdate()
    {
        if (!enemy.CanSeePlayer)
        {
            stateMachine.ChangeState(enemy.ReturnState);
            return;
        }

        if (!enemy.CanAttackPlayer)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // Поворачиваемся в сторону игрока
        FacePlayer();

        if (Time.time >= lastAttackTime + enemy.AttackCooldown)
        {
            ShootAtPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void FacePlayer()
    {
        if (enemy.PlayerTransform == null)
            return;

        Vector2 direction = (enemy.PlayerTransform.position - enemy.transform.position).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            enemy.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void ShootAtPlayer()
    {
        if (enemy.PlayerTransform == null || enemy.BulletPrefab == null)
            return;

        // Определяем позицию спавна пули
        Vector3 spawnPosition = enemy.BulletSpawnPoint != null 
            ? enemy.BulletSpawnPoint.position 
            : enemy.transform.position;

        // Создаем пулю в точке спавна
        GameObject bulletObj = Object.Instantiate(
            enemy.BulletPrefab, 
            spawnPosition, 
            Quaternion.identity
        );

        // Вычисляем направление на игрока
        Vector2 direction = (enemy.PlayerTransform.position - spawnPosition).normalized;

        // Если есть компонент Bullet, устанавливаем направление
        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.SetDirection(direction);
            bullet.SetSourceEnemy(enemy.gameObject);
        }
        else
        {
            // Если это просто объект с Rigidbody2D
            Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * 10f; // Скорость пули
            }
        }

        Debug.Log($"{enemy.name} выстрелил в сторону {direction}");
    }
}
