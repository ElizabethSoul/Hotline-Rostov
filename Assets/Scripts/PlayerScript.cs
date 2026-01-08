using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private Vector2 mousePosition;
    private Camera mainCamera;

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
        // === Поворот персонажа в сторону мыши ===
        Vector3 screenPoint = new Vector3(mousePosition.x, mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z);
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(screenPoint);

        Vector2 directionToMouse = ((Vector2)worldMousePos - rb.position).normalized;

        if (directionToMouse.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // === Движение: фиксированное по миру (WASD относительно экрана) ===
        // moveInput.x — A/D (влево/вправо по горизонтали мира)
        // moveInput.y — W/S (вверх/вниз по вертикали мира)
        Vector2 moveDirection = moveInput;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);

        // Анимация ходьбы (по факту движения)
        if (animator != null)
        {
            animator.SetBool("IsWalking", moveDirection.sqrMagnitude > 0.01f);
        }
    }
}