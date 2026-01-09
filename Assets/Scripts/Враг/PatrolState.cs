using UnityEngine;

public class PatrolState : EnemyState
{
    private Transform[] points;
    private int currentIndex;
    private float waitTimer;

    public PatrolState(Enemy enemy, EnemyStateMachine stateMachine, Transform[] patrolPoints)
        : base(enemy, stateMachine)
    {
        points = patrolPoints;
        currentIndex = GetClosestIndex();
    }

    public override void Enter()
    {
        waitTimer = 0f;
    }

    public override void LogicUpdate()
    {
        if (enemy.CanSeePlayer)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (points == null || points.Length == 0)
            stateMachine.ChangeState(enemy.IdleState);
    }

    public override void PhysicsUpdate()
    {
        if (points == null || points.Length == 0) return;

        Vector3 target = points[currentIndex].position;
        float dist = Vector3.Distance(enemy.transform.position, target);

        if (dist < 0.3f)
        {
            enemy.Stop();
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= enemy.WaitAtPoint)
            {
                waitTimer = 0f;
                currentIndex = (currentIndex + 1) % points.Length;
                if (!enemy.LoopPatrol && currentIndex == 0) currentIndex = points.Length - 1;
            }
        }
        else
        {
            waitTimer = 0f;
            enemy.MoveTowards(target, enemy.PatrolSpeed);
        }
    }

    private int GetClosestIndex()
    {
        if (points.Length == 0) return 0;
        int closest = 0;
        float minDist = Vector3.Distance(enemy.transform.position, points[0].position);
        for (int i = 1; i < points.Length; i++)
        {
            float d = Vector3.Distance(enemy.transform.position, points[i].position);
            if (d < minDist) { minDist = d; closest = i; }
        }
        return closest;
    }
}