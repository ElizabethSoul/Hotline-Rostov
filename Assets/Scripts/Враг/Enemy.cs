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

    // Компоненты и ссылки
    private NavMeshAgent agent;
    private Rigidbody2D rb2d;
    [SerializeField] private Animator animator;
    private static readonly int WalkHash = Animator.StringToHash("isWalking");
    public Transform PlayerTransform { get; private set; }
    private Vector2 homePosition;

    // Состояния
    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public RangeAttackState RangeAttackState { get; private set; }
    public ReturnState ReturnState { get; private set; }

    // Определяемое состояние атаки в зависимости от типа
    public EnemyState CurrentAttackState 
    { 
        get { return attackType == AttackType.Melee ? AttackState : (EnemyState)RangeAttackState; } 
    }

    // Радиус атаки в зависимости от типа
    public float CurrentAttackRange
    {
        get { return attackType == AttackType.Melee ? attackRange : rangedAttackRange; }
    }

    private EnemyStateMachine stateMachine;
    private float currentHealth;
    private DropItems dropItems;

    // Публичные свойства для состояний
    public bool UsePatrol => usePatrol;                     // Новый геттер
public Transform[] PatrolPoints => patrolPoints;        // Новый геттер (для точек патруля)
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

    private bool PlayerInRange(float range)
    {
        return PlayerTransform != null && 
               Vector2.Distance(transform.position, PlayerTransform.position) <= range;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb2d = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb2d != null)
        {
            rb2d.gravityScale = 0f; // top-down: отключаем гравитацию у тела
        }
        agent.updateRotation = false;
        #if UNITY_2019_1_OR_NEWER
        try { agent.updateUpAxis = false; } catch {}
        #endif
        // Отключаем автоматическое перемещение агента — будем двигать через Rigidbody2D
        try { agent.updatePosition = false; } catch {}
        // На всякий случай обнулим baseOffset для 2D
        try { agent.baseOffset = 0f; } catch {}
    }

    private void Start()
    {
        currentHealth = maxHealth;
        homePosition = transform.position;
        dropItems = GetComponent<DropItems>();

        PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (PlayerTransform == null)
            Debug.LogError("[Enemy] Игрок с тегом 'Player' не найден!");

        stateMachine = new EnemyStateMachine();

        IdleState    = new IdleState(this, stateMachine);
        PatrolState  = new PatrolState(this, stateMachine, patrolPoints);
        ChaseState   = new ChaseState(this, stateMachine);
        AttackState  = new AttackState(this, stateMachine);
        RangeAttackState = new RangeAttackState(this, stateMachine);
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
        // Синхронизируем расчетную позицию агента с физическим телом (для 2D)
        if (agent != null && rb2d != null)
        {
            Vector3 next = agent.nextPosition;
            rb2d.MovePosition(new Vector2(next.x, next.y));
        }
    }

    // Движение
    public void MoveTowards(Vector3 target, float speed)
    {
        if (agent == null) return;
        agent.speed = speed;
        agent.isStopped = false;
        agent.SetDestination(new Vector3(target.x, target.y, 0f));
        if (animator != null)
            animator.SetBool(WalkHash, true);
    }

    public void Stop()
    {
        if (agent == null) return;
        agent.ResetPath();
        agent.isStopped = true;
        if (animator != null)
            animator.SetBool(WalkHash, false);
    }

    // Поворот в сторону движения (как у PlayerScript)
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

    // Урон от пули (вызывается из Bullet.cs)
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // уже мёртв

        currentHealth -= amount;
        Debug.Log($"{name} получил {amount} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
        else
            stateMachine.ChangeState(ChaseState); // агримся при попадании
    }

    private void Die()
    {
        // Остановить поведение
        Stop();

        // Отключаем анимацию и все коллайдеры, чтобы объект стал неинтерактивным
        if (animator != null)
            animator.SetBool(WalkHash, false);

        // Отключаем все 2D-коллайдеры на объекте и дочерних объектах
        Collider2D[] coll2D = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in coll2D)
            c.enabled = false;

        // Отключаем все 3D-коллайдеры на объекте и дочерних объектах
        Collider[] coll3D = GetComponentsInChildren<Collider>(true);
        foreach (var c in coll3D)
            c.enabled = false;

        // Отключаем навмеш-агент и состояние врага
        if (agent != null)
        {
            try { agent.enabled = false; } catch {}
        }

        // Отключаем сам скрипт состояния, чтобы предотвратить дальнейшие взаимодействия
        try { this.enabled = false; } catch {}

        // Выпадение предметов при смерти
        if (dropItems != null)
            dropItems.DropEnemyItems(transform.position);

        // Удаляем объект через некоторое время (даём время эффектам/предметам)
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