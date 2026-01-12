using UnityEngine;

public class ObjectsInteraction : MonoBehaviour
{
    protected Animator animator;
    protected bool isPlayerInRange = false;
    private enum ObjectType
    {
        Coin,
        HealthPack
    }
    [SerializeField]
    private ObjectType objectType = ObjectType.HealthPack;
    [SerializeField]
    private int coinAmount;
    [SerializeField]
    private int healthAmount;
    private PlayerScript player;
    private void Start()
    {
        animator = GetComponent<Animator>();

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (animator != null)
                animator.SetBool("Pick", true);
            player = collision.GetComponent<PlayerScript>();
            if (player != null)
                player.SetCurrentInteractable(this);
            Debug.Log($"[ObjectsInteraction] Игрок вошел в область взаимодействия с {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = collision.GetComponent<PlayerScript>();
            if (player != null)
                player.SetCurrentInteractable(null);

            if (animator != null)
                animator.SetBool("Pick", false);

            Debug.Log($"[ObjectsInteraction] Игрок вышел из области взаимодействия с {gameObject.name}");
        }
    }

    public virtual void Pickup()
    {
        if (!isPlayerInRange)
            return;

        Debug.Log($"[ObjectsInteraction] Объект {gameObject.name} подобран!");
        switch (objectType)
        {
            case ObjectType.Coin:
                CurrencyManager.AddCoins(coinAmount);
                Debug.Log("[ObjectsInteraction] Подобрана монета!");
                break;
            case ObjectType.HealthPack:
                player.Heal(healthAmount);
                Debug.Log("[ObjectsInteraction] Подобрана аптечка!");
                break;
            default:
                Debug.LogWarning("[ObjectsInteraction] Неизвестный тип объекта при подборе!");
                break;
        }
        if (animator != null)
            animator.SetBool("Pick", false);

        Destroy(gameObject);
    }
}
