using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private Vector2 mousePosition;
    private Camera mainCamera;
    private Vector2 facingDirection = Vector2.right; // Направление взгляда (по умолчанию вправо)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
    }

    public void OnPointerPosition(InputAction.CallbackContext ctx)
    {
        mousePosition = ctx.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // ==================== Направление к курсору ====================
Vector3 screenPoint = new Vector3(mousePosition.x, mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z);
Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(screenPoint);

// Исправленная строка — выбирай один из вариантов:
Vector2 directionToMouse = ((Vector2)worldMousePos - rb.position).normalized;  // Рекомендую этот

// Обновляем facing только если курсор не прямо на персонаже
if (directionToMouse.sqrMagnitude > 0.01f)
{
    facingDirection = directionToMouse;

    float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(0, 0, angle);
}
        // ==================== Движение относительно направления взгляда ====================
        // forward * Y (W/S) + strafeRight * X (A/D)
        Vector2 forward = facingDirection;
        Vector2 strafeRight = new Vector2(forward.y, -forward.x); // Правый стрейф (90° по часовой)
        Vector2 moveDirection = forward * moveInput.y + strafeRight * moveInput.x;

        // Нормализация для равной скорости по диагонали
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        // Применяем
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);

        // Анимация
        if (animator != null)
            animator.SetBool("IsWalking", moveDirection != Vector2.zero);
    }
}