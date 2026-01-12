using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private Image hpBarImage;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Canvas DeadCanvas;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;
        
        // Инициализируем HP bar, если он назначен в инспекторе
        if (hpBarImage != null)
            UpdateHPBar();

        // Инициализируем отображение монет и подписываемся на изменения
        if (coinsText != null)
        {
            UpdateCoinsDisplay(CurrencyManager.GetCoins());
            CurrencyManager.OnCoinsChanged += UpdateCoinsDisplay;
        }
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
        
        UpdateHPBar();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"[Player] Восстановлено здоровье: {amount}. Текущее здоровье: {currentHealth}/{maxHealth}");
        
        UpdateHPBar();
    }

    public void RestoreToMaxHealth()
    {
        currentHealth = maxHealth;
        Debug.Log($"[Player] Здоровье восстановлено до максимального: {currentHealth}/{maxHealth}");
        UpdateHPBar();
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (amount <= 0f) return;
        maxHealth += amount;
        // Optionally increase current health so player benefits immediately
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"[Player] Базовое здоровье увеличено на {amount}. Сейчас: {currentHealth}/{maxHealth}");
        UpdateHPBar();
    }

    // Accessors for saving/loading
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetMaxHealth(float value)
    {
        if (value <= 0f) return;
        maxHealth = value;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHPBar();
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (hpBarImage != null)
        {
            hpBarImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }

    private void UpdateCoinsDisplay(int coinAmount)
    {
        if (coinsText != null)
        {
            coinsText.text = $"{coinAmount}";
        }
    }

    private void Die()
    {
        DeadCanvas.gameObject.SetActive(true);
        Debug.Log("[Player] Игрок погиб!");
        gameObject.SetActive(false);
    }
}