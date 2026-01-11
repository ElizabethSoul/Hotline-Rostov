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
        if (!enemy.CanSeePlayer)
        {
            stateMachine.ChangeState(enemy.ReturnState);
            return;
        }

        // Ближняя атака: ожидаем срабатывания триггерного хитбокса
        if (enemy.AttackTypeEnum == AttackType.Melee)
        {
            if (!enemy.IsPlayerInMeleeRange)
            {
                // двигаться к игроку
                if (enemy.PlayerTransform != null)
                {
                    enemy.MoveTowards(enemy.PlayerTransform.position, enemy.ChaseSpeed);
                    return;
                }
            }
            else
            {
                enemy.Stop();
            }
        }

        if (Time.time >= lastAttackTime + enemy.AttackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void AttackPlayer()
    {
        // Запустить анимацию ближней атаки
        enemy.TriggerMeleeAttack();
        if (enemy.PlayerTransform.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage((int)enemy.AttackDamage);
        }
    }
}