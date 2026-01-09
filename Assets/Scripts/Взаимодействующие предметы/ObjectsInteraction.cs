using UnityEngine;

public class ObjectsInteraction : MonoBehaviour
{
    protected Animator animator;
    protected bool isPlayerInRange = false;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Убедимся что коллайдер это триггер
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем по тегу что это игрок
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            // Ставим флажок Pick в аниматоре
            if (animator != null)
                animator.SetBool("Pick", true);

            // Передаем ссылку на этот объект в PlayerScript
            PlayerScript player = collision.GetComponent<PlayerScript>();
            if (player != null)
                player.SetCurrentInteractable(this);

            Debug.Log($"[ObjectsInteraction] Игрок вошел в область взаимодействия с {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Проверяем по тегу что это игрок
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
// Удаляем ссылку на этот объект из PlayerScript
            PlayerScript player = collision.GetComponent<PlayerScript>();
            if (player != null)
                player.SetCurrentInteractable(null);

            
            // Снимаем флажок Pick в аниматоре
            if (animator != null)
                animator.SetBool("Pick", false);

            Debug.Log($"[ObjectsInteraction] Игрок вышел из области взаимодействия с {gameObject.name}");
        }
    }

    /// <summary>
    /// Подбирает объект и удаляет его с карты.
    /// Вызывается из системы ввода (Input System) когда игрок нажимает кнопку взаимодействия.
    /// </summary>
    public virtual void Pickup()
    {
        if (!isPlayerInRange)
            return;

        Debug.Log($"[ObjectsInteraction] Объект {gameObject.name} подобран!");
        
        // Снимаем флажок перед уничтожением
        if (animator != null)
            animator.SetBool("Pick", false);

        // Уничтожаем объект
        Destroy(gameObject);
    }
}
