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
        if (enemy.PlayerTransform.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage((int)enemy.AttackDamage);
        }
    }
}