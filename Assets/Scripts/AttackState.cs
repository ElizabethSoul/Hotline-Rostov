using UnityEngine;

public class AttackState : EnemyState
{
    private float lastAttackTime;

    public AttackState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
        lastAttackTime = -999f;
    }

    public override void Enter()
    {
        enemy.Stop();
    }

    public override void LogicUpdate()
    {
        if (!enemy.CanAttackPlayer)
        {
            if (enemy.CanSeePlayer)
                stateMachine.ChangeState(enemy.ChaseState);
            else
                stateMachine.ChangeState(enemy.ReturnState);
            return;
        }

        if (Time.time >= lastAttackTime + enemy.AttackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void AttackPlayer()
    {
        // Здесь можно добавить триггер анимации: enemy.Animator.SetTrigger("Attack");

        // Наносим урон игроку — ищем любой компонент с TakeDamage
        if (enemy.PlayerTransform.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage((int)enemy.AttackDamage);
        }
        // Или конкретно PlayerHealth, если есть
    }
    public interface IDamageable
{
    void TakeDamage(int amount);
}
}