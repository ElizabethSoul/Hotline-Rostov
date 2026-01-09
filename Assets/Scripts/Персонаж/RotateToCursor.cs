using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAndLookAtMouse : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;          // Скорость движения
    [SerializeField] private string mousePositionActionName = "MousePosition"; // Точное имя твоего действия!

    private PlayerInput playerInput;
    private InputAction mousePositionAction;
    private Camera mainCamera;
    private Rigidbody2D rb;

    private void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        // Находим действие по имени
        mousePositionAction = playerInput.actions[mousePositionActionName];

        if (mousePositionAction == null)
        {
            Debug.LogError($"Действие с именем '{mousePositionActionName}' не найдено! Проверь имя в Input Actions.");
        }
    }

    private void OnEnable()
    {
        mousePositionAction.Enable();
    }

    private void OnDisable()
    {
        mousePositionAction.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 mouseScreenPos = mousePositionAction.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 direction = mouseWorldPos - transform.position;

        // Поворот
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Движение всегда за курсором (если хочешь только при нажатии — скажи, добавлю)
        if (direction.sqrMagnitude > 0.1f)
        {
            Vector2 velocity = direction.normalized * moveSpeed;

            if (rb != null)
                rb.linearVelocity = velocity;
            else
                transform.position += (Vector3)velocity * Time.fixedDeltaTime;
        }
        else
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
}