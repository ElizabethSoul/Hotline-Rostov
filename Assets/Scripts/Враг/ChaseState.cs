public class ChaseState : EnemyState
{
    public ChaseState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void LogicUpdate()
    {
        if (!enemy.CanSeePlayer)
        {
            stateMachine.ChangeState(enemy.ReturnState);
            return;
        }

        if (enemy.CanAttackPlayer)
        {
            stateMachine.ChangeState(enemy.CurrentAttackState);
        }
    }

    public override void PhysicsUpdate()
    {
        if (enemy.PlayerTransform != null)
            enemy.MoveTowards(enemy.PlayerTransform.position, enemy.ChaseSpeed);
    }
}