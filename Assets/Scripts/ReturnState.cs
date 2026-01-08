using UnityEngine;
public class ReturnState : EnemyState
{
    private Vector2 homePosition;

    public ReturnState(Enemy enemy, EnemyStateMachine stateMachine, Vector2 home)
        : base(enemy, stateMachine)
    {
        homePosition = home;
    }

    public override void LogicUpdate()
    {
        if (enemy.CanSeePlayer)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (Vector2.Distance(enemy.transform.position, homePosition) < 0.3f)
        {
            // ИСПРАВЛЕНО: используем публичные свойства
            if (enemy.UsePatrol && enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0)
                stateMachine.ChangeState(enemy.PatrolState);
            else
                stateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        enemy.MoveTowards(homePosition, enemy.PatrolSpeed);
    }
}