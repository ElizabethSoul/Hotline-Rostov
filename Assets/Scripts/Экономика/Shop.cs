using UnityEngine;
using System.Collections;

public class Shop : ObjectsInteraction
{
    [Header("Shop UI")]
    [SerializeField] private GameObject shopCanvas;

    [Header("Товары и цены")]
    [Tooltip("Цены товаров. Элемент с индексом 0 — цена первого товара, 1 — второго и т.д.")]
    [SerializeField] private int[] itemPrices = new int[0];

    [Header("Окна")] 
    [Tooltip("Окно, показываемое при недостатке монет (можно задать панель с изображением).")]
    [SerializeField] private GameObject notEnoughWindow;

    public override void Pickup()
    {
        if (!isPlayerInRange)
            return;

        if (shopCanvas != null)
        {
            shopCanvas.SetActive(true);
            Debug.Log($"[Shop] Открыт магазин {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[Shop] Для {gameObject.name} не назначен Shop Canvas");
        }
    }
    public void CloseShop()
    {
        if (shopCanvas != null) 
            shopCanvas.SetActive(false);
    }
    public void Purchase(int index)
    {
        if (index < 0 || index >= itemPrices.Length)
        {
            Debug.LogWarning($"[Shop] Неверный индекс товара: {index}");
            return;
        }

        int price = itemPrices[index];
        int current = CurrencyManager.GetCoins();

        if (current < price)
        {
            if (notEnoughWindow != null)
            {
                notEnoughWindow.SetActive(true);
                StopAllCoroutines(); 
                StartCoroutine(HideNotEnoughWindowAfterDelay());
            }
            
            Debug.Log($"[Shop] Недостаточно монет для покупки товара {index}. Нужна: {price}, есть: {current}");
            return;
        }
        CurrencyManager.AddCoins(-price);
        Debug.Log($"[Shop] Покупка совершена: товар {index}, стоимость {price}");

        // Применить эффект товара
        ApplyItemEffect(index);
    }

    private IEnumerator HideNotEnoughWindowAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (notEnoughWindow != null)
        {
            notEnoughWindow.SetActive(false);
        }
    }
    private void ApplyItemEffect(int index)
    {
        switch (index)
        {
            case 0:
                RestoreHP();
                break;
            case 1:
                IncreaseAttackSpeed();
                break;
            case 2:
                IncreaseBulletDamage();
                break;
            default:
                Debug.LogWarning($"[Shop] Неизвестный эффект для товара {index}");
                break;
        }
    }

    public void RestoreHP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerScript ps = player.GetComponent<PlayerScript>();
            if (ps != null)
            {
                ps.RestoreToMaxHealth();
                Debug.Log("[Shop] Здоровье игрока восстановлено до максимального");
            }
        }
    }

    public void IncreaseAttackSpeed()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pa != null)
            {
                pa.IncreaseAttackSpeed(0.1f); // уменьшить cooldown на 0.1
                Debug.Log("[Shop] Скорость атаки игрока повышена");
            }
        }
    }

    public void IncreaseBulletDamage()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pa != null)
            {
                pa.IncreaseBulletDamage(5); // увеличить урон на 5
                Debug.Log("[Shop] Урон от пуль игрока повышен");
            }
        }
    }
}