using UnityEngine;
using UnityEngine.InputSystem;

public interface IDamageable
{
    void TakeDamage(int amount);
}
public class PlayerScript : MonoBehaviour, IDamageable
{
    public float speed = 5f;
    [SerializeField] private float maxHealth = 100f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private Vector2 mousePosition;
    private Camera mainCamera;
    private float currentHealth;
    private ObjectsInteraction currentInteractable;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;
    }

    public void OnPointerPosition(InputAction.CallbackContext ctx)
    {
        mousePosition = ctx.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentInteractable != null)
        {
            currentInteractable.Pickup();
        }
    }

    public void SetCurrentInteractable(ObjectsInteraction interactable)
    {
        currentInteractable = interactable;
    }

    void FixedUpdate()
    {
        Vector3 screenPoint = new Vector3(mousePosition.x, mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z);
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(screenPoint);

        Vector2 directionToMouse = ((Vector2)worldMousePos - rb.position).normalized;

        if (directionToMouse.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        Vector2 moveDirection = moveInput;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);

        if (animator != null)
        {
            animator.SetBool("IsWalking", moveDirection.sqrMagnitude > 0.01f);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"[Player] Получен урон: {amount}. Здоровье: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Player] Игрок погиб!");
        gameObject.SetActive(false);
    }
}