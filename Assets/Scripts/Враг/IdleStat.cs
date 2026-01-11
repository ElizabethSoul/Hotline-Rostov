using UnityEngine;

public class IdleState : EnemyState
{
    private float waitTimer;
    private Vector3 target;
    private bool hasTarget;

    public IdleState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.Stop();
        waitTimer = 0f;
        hasTarget = false;
    }

    public override void LogicUpdate()
    {
        if (enemy.CanSeePlayer)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // Если у врага патрульные точки есть, переключаемся в патруль (без действий здесь)
        if (enemy.UsePatrol && enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0)
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        // Если нет патруля — блуждаем по NavMesh
        if (!hasTarget)
        {
            Vector3 dest;
            if (enemy.TryGetRandomNavMeshPosition(out dest))
            {
                target = dest;
                hasTarget = true;
            }
        }
        else
        {
            float dist = Vector3.Distance(enemy.transform.position, target);
            if (dist < 0.35f)
            {
                enemy.Stop();
                waitTimer += Time.deltaTime;
                if (waitTimer >= enemy.WaitAtPoint && enemy.TryGetRandomNavMeshPosition(out target))
                {
                    waitTimer = 0f;
                    enemy.MoveTowards(target, enemy.PatrolSpeed);
                }
            }
            else
            {
                // продолжаем двигаться к цели
                enemy.MoveTowards(target, enemy.PatrolSpeed);
            }
        }
    }
}