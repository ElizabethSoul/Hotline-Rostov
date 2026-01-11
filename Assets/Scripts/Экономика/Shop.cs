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
    }

    private IEnumerator HideNotEnoughWindowAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        if (notEnoughWindow != null)
            notEnoughWindow.SetActive(false);
    }
    public void CloseNotEnoughWindow()
    {
        StopAllCoroutines();
        if (notEnoughWindow != null) 
            notEnoughWindow.SetActive(false);
    }
}