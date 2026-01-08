using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Health & Damage")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.3f;

    [Header("Patrol")]
    [SerializeField] private bool usePatrol = true;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool loopPatrol = true;
    [SerializeField] private float waitAtPoint = 1f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    // Компоненты и ссылки
    private Rigidbody2D rb;
    public Transform PlayerTransform { get; private set; }
    private Vector2 homePosition;

    // Состояния
    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public ReturnState ReturnState { get; private set; }

    private EnemyStateMachine stateMachine;
    private float currentHealth;

    // Публичные свойства для состояний
    public bool UsePatrol => usePatrol;                     // Новый геттер
public Transform[] PatrolPoints => patrolPoints;        // Новый геттер (для точек патруля)
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float WaitAtPoint => waitAtPoint;
    public bool LoopPatrol => loopPatrol;

    public bool CanSeePlayer => PlayerInRange(detectionRange);
    public bool CanAttackPlayer => PlayerInRange(attackRange);

    private bool PlayerInRange(float range)
    {
        return PlayerTransform != null && 
               Vector2.Distance(transform.position, PlayerTransform.position) <= range;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        homePosition = transform.position;

        PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (PlayerTransform == null)
            Debug.LogError("[Enemy] Игрок с тегом 'Player' не найден!");

        stateMachine = new EnemyStateMachine();

        IdleState    = new IdleState(this, stateMachine);
        PatrolState  = new PatrolState(this, stateMachine, patrolPoints);
        ChaseState   = new ChaseState(this, stateMachine);
        AttackState  = new AttackState(this, stateMachine);
        ReturnState  = new ReturnState(this, stateMachine, homePosition);

        if (usePatrol && patrolPoints != null && patrolPoints.Length > 0)
            stateMachine.Initialize(PatrolState);
        else
            stateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (currentHealth <= 0) return;
        stateMachine.CurrentState.LogicUpdate();
        FaceMovementDirection(); // top-down поворот как у игрока
    }

    private void FixedUpdate()
    {
        if (currentHealth <= 0) return;
        stateMachine.CurrentState.PhysicsUpdate();
    }

    // Движение
    public void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    // Поворот в сторону движения (как у PlayerScript)
    private void FaceMovementDirection()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Урон от пули (вызывается из Bullet.cs)
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} получил {amount} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
        else
            stateMachine.ChangeState(ChaseState); // агримся при попадании
    }

    private void Die()
    {
        Stop();
        GetComponent<Collider2D>().enabled = false;
        // Можно добавить анимацию смерти
        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

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