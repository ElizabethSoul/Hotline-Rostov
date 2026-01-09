public class IdleState : EnemyState
{
    public IdleState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.Stop();
    }

    public override void LogicUpdate()
    {
        if (enemy.CanSeePlayer)
            stateMachine.ChangeState(enemy.ChaseState);
    }
}