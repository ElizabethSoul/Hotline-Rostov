using UnityEngine;
using UnityEngine.AI;
public enum AttackType { Melee, Ranged }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DropItems))]
public class Enemy : MonoBehaviour
{
    [Header("Health & Damage")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private AttackType attackType = AttackType.Melee;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float chaseSpeed = 3.5f;
    
    [Header("Патруль / Блуждание")]
    [Tooltip("Радиус, в пределах которого враг будет случайно блуждать по NavMesh, если у него нет патрульных точек.")]
    [SerializeField] private float wanderRadius = 8f;

    [Tooltip("Минимальная дистанция от текущей позиции до выбранной точки блуждания (чтобы не выбирать слишком близкие точки).")]
    [SerializeField] private float wanderMinDistance = 2f;

    [Tooltip("Максимальное число попыток найти корректную точку на NavMesh.")]
    [SerializeField] private int wanderAttempts = 20;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float rangedAttackRange = 4f;

    [Header("Patrol")]
    [SerializeField] private bool usePatrol = true;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool loopPatrol = true;
    [SerializeField] private float waitAtPoint = 1f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private NavMeshAgent agent;
    private Rigidbody2D rb2d;
    private Animator animator;
    [Header("Анимации")]
    [Tooltip("Имя триггера аниматора для ближней атаки.")]
    [SerializeField] private string meleeAttackTrigger = "Attack";
    private static readonly int WalkHash = Animator.StringToHash("isWalking");
    public Transform PlayerTransform { get; private set; }
    private Vector2 homePosition;

    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public RangeAttackState RangeAttackState { get; private set; }
    public ReturnState ReturnState { get; private set; }

    public EnemyState CurrentAttackState 
    { 
        get { return attackType == AttackType.Melee ? AttackState : (EnemyState)RangeAttackState; } 
    }

    public float CurrentAttackRange
    {
        get { return attackType == AttackType.Melee ? attackRange : rangedAttackRange; }
    }

    private EnemyStateMachine stateMachine;
    private float currentHealth;
    private DropItems dropItems;
    private bool isPlayerInMeleeRange = false;

    public bool UsePatrol => usePatrol;  
    public Transform[] PatrolPoints => patrolPoints; 
    public float RangedAttackRange => rangedAttackRange;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float WaitAtPoint => waitAtPoint;
    public bool LoopPatrol => loopPatrol;
    public GameObject BulletPrefab => bulletPrefab;
    public Transform BulletSpawnPoint => bulletSpawnPoint;
    public AttackType AttackTypeEnum => attackType;

    public bool CanSeePlayer => PlayerInRange(detectionRange);
    public bool CanAttackPlayer => PlayerInRange(CurrentAttackRange);
    public bool IsPlayerInMeleeRange => isPlayerInMeleeRange;

    private bool PlayerInRange(float range)
    {
        return PlayerTransform != null && 
               Vector2.Distance(transform.position, PlayerTransform.position) <= range;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        try { rb2d.freezeRotation = true; } catch {}
        agent.updateRotation = false;
        try { agent.updateUpAxis = false; } catch {}
        try { agent.updatePosition = false; } catch {}
        try { agent.baseOffset = 0f; } catch {}
        // Убираем инерцию для мгновенного разгона и торможения
        agent.acceleration = 100f;
    }

    private void Start()
    {
        // Apply difficulty modifiers from SettingsManager (if available)
        if (SettingsManager.Instance != null)
        {
            maxHealth = SettingsManager.Instance.GetEnemyHP(Mathf.RoundToInt(maxHealth));
            attackDamage = SettingsManager.Instance.GetEnemyAttack(Mathf.RoundToInt(attackDamage));
        }

        currentHealth = maxHealth;
        homePosition = (Vector2)transform.position;
        dropItems = GetComponent<DropItems>();
        PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        stateMachine = new EnemyStateMachine();
        IdleState = new IdleState(this, stateMachine);
        PatrolState = new PatrolState(this, stateMachine, patrolPoints);
        ChaseState = new ChaseState(this, stateMachine);
        AttackState = new AttackState(this, stateMachine);
        RangeAttackState = new RangeAttackState(this, stateMachine);
        ReturnState = new ReturnState(this, stateMachine, homePosition);

        if (usePatrol && patrolPoints != null && patrolPoints.Length > 0)
            stateMachine.Initialize(PatrolState);
        else
            stateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (currentHealth <= 0) return;
        stateMachine.CurrentState.LogicUpdate();
        FaceMovementDirection();
    }

    private void FixedUpdate()
    {
        if (currentHealth <= 0) return;
        stateMachine.CurrentState.PhysicsUpdate();
        if (agent != null && rb2d != null)
        {
            float syncDistance = Mathf.Max(0.5f, agent.radius * 0.5f);
            if (Vector3.Distance(transform.position, agent.nextPosition) > syncDistance)
            {
                agent.nextPosition = transform.position;
            }

            Vector3 next = agent.nextPosition;
            rb2d.MovePosition(new Vector2(next.x, next.y));
        }
    }
    public void MoveTowards(Vector3 target, float speed)
    {
        if (agent == null) return;
        agent.speed = speed;
        agent.isStopped = false;
        agent.SetDestination(new Vector3(target.x, target.y, 0f));
        animator.SetBool(WalkHash, true);
    }

    public void TriggerMeleeAttack()
    {
        if (!string.IsNullOrEmpty(meleeAttackTrigger))
            animator.SetTrigger(meleeAttackTrigger);
    }

    public void SetPlayerInMeleeRange(bool value)
    {
        isPlayerInMeleeRange = value;
        if (value)
        {
            stateMachine.ChangeState(AttackState);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInMeleeRange = true;
            stateMachine?.ChangeState(AttackState);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInMeleeRange = false;
        }
    }

    public bool TryGetRandomNavMeshPosition(out Vector3 result)
    {
        Vector3 origin = transform.position;
        if (wanderRadius <= 0f)
        {
            result = origin;
            return false;
        }

        NavMeshPath path = new();
        for (int i = 0; i < Mathf.Max(1, wanderAttempts); i++)
        {
            float dist = Random.Range(Mathf.Max(0f, wanderMinDistance), wanderRadius);
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 sample = origin + new Vector3(dir.x * dist, dir.y * dist, 0f);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(sample, out hit, 1.5f, NavMesh.AllAreas))
                continue;

            Vector3 candidate = hit.position;

            if (agent != null)
            {
                agent.CalculatePath(candidate, path);
                if (path.status != NavMeshPathStatus.PathComplete)
                    continue;

                // вычисляем длину пути
                float pathLength = 0f;
                if (path.corners.Length >= 2)
                {
                    for (int c = 1; c < path.corners.Length; c++)
                        pathLength += Vector3.Distance(path.corners[c - 1], path.corners[c]);
                }
                if (pathLength < wanderMinDistance)
                    continue;
            }

            result = candidate;
            return true;
        }

        result = origin;
        return false;
    }

    public void Stop()
    {
        if (agent == null) return;
        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool(WalkHash, false);
    }
    private void FaceMovementDirection()
    {
        if (agent == null) return;
        Vector3 vel = agent.velocity;
        if (vel.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= amount;
        Debug.Log($"{name} получил {amount} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
        else
            stateMachine.ChangeState(ChaseState);
    }

    private void Die()
    {
        Stop();
        animator.SetBool(WalkHash, false);
        Collider2D[] coll2D = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in coll2D)
            c.enabled = false;

        Collider[] coll3D = GetComponentsInChildren<Collider>(true);
        foreach (var c in coll3D)
            c.enabled = false;

        if (agent != null)
        {
            try { agent.enabled = false; } catch {}
        }

        try { this.enabled = false; } catch {}

        if (dropItems != null)
            dropItems.DropEnemyItems(transform.position);

        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (attackType == AttackType.Melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
        }

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);
            }
        }
    }
    
}