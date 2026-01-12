using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class Shop : ObjectsInteraction
{
    [Header("Shop UI")]
    [SerializeField] private GameObject shopCanvas;

    [Header("Товары и цены")]
    [Tooltip("Цены товаров. Элемент с индексом 0 — цена первого товара, 1 — второго и т.д.")]
    [SerializeField] private int[] itemPrices = new int[0];
    [Tooltip("Текущий уровень усиления для каждого товара (0..maxLevel). Если не заполнено, инициализируется автоматически.)")]
    [SerializeField] private int[] currentLevels = new int[0];
    [Tooltip("Максимальный уровень прокачки для каждого усиления")]
    [SerializeField] private int maxLevel = 10;
    

    [Header("Отдельные прогресс-бары (опционально)")]
    [Tooltip("Прогресс-бар для восстановления здоровья (товар 0)")]
    [SerializeField] private Image healthProgressBar;
    [Tooltip("Прогресс-бар для скорости атаки (товар 1)")]
    [SerializeField] private Image attackSpeedProgressBar;
    [Tooltip("Прогресс-бар для силы атаки (товар 2)")]
    [SerializeField] private Image damageProgressBar;

    [Header("UI: сообщения")]
    [Tooltip("Панель, показываемая при достижении максимального уровня (скрывается через 3 сек).")]
    [SerializeField] private GameObject maxReachedPanel;
    [Tooltip("Панель/текст для отображения текущего количества монет")]
    [SerializeField] private TextMeshProUGUI coinsPanelText;

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
    private void OnEnable()
    {
        // ensure arrays align with item prices
        if (currentLevels == null || currentLevels.Length != itemPrices.Length)
        {
            currentLevels = new int[itemPrices.Length];
        }
        LoadLevels();
        UpdateAllProgressBars();
        UpdateCoinsDisplay(CurrencyManager.GetCoins());
        CurrencyManager.OnCoinsChanged += UpdateCoinsDisplay;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCoinsChanged -= UpdateCoinsDisplay;
    }

    private void UpdateCoinsDisplay(int amount)
    {
        if (coinsPanelText != null)
            coinsPanelText.text = amount.ToString();
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

        // check max level
        if (currentLevels != null && index < currentLevels.Length && currentLevels[index] >= maxLevel)
        {
            if (maxReachedPanel != null)
            {
                maxReachedPanel.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(HideMaxReachedPanelAfterDelay());
            }
            Debug.Log($"[Shop] Достигнут максимальный уровень для товара {index}");
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

        // increase level and update UI
        if (currentLevels != null && index < currentLevels.Length)
        {
            currentLevels[index] = Mathf.Min(maxLevel, currentLevels[index] + 1);
            UpdateProgressBar(index);
            SaveLevels();
        }
    }

    private IEnumerator HideNotEnoughWindowAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (notEnoughWindow != null)
        {
            notEnoughWindow.SetActive(false);
        }
    }

    private IEnumerator HideMaxReachedPanelAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (maxReachedPanel != null)
            maxReachedPanel.SetActive(false);
    }

    private void UpdateAllProgressBars()
    {
        // Update the three explicit progress bars (health, attack speed, damage)
        UpdateProgressBar(0);
        UpdateProgressBar(1);
        UpdateProgressBar(2);
    }

    private void UpdateProgressBar(int index)
    {
        if (currentLevels == null || index < 0 || index >= currentLevels.Length) return;

        Image img = null;
        // prefer explicit individual bars for the three boosts
        if (index == 0 && healthProgressBar != null) img = healthProgressBar;
        else if (index == 1 && attackSpeedProgressBar != null) img = attackSpeedProgressBar;
        else if (index == 2 && damageProgressBar != null) img = damageProgressBar;

        if (img == null) return;
        img.fillAmount = maxLevel > 0 ? (float)currentLevels[index] / (float)maxLevel : 0f;
    }

    // --- Save / Load levels per shop instance ---
    private string GetLevelKey(int index)
    {
        return $"shop_{gameObject.name}_level_{index}";
    }

    private void SaveLevels()
    {
        if (currentLevels == null) return;
        for (int i = 0; i < currentLevels.Length; i++)
        {
            PlayerPrefs.SetInt(GetLevelKey(i), currentLevels[i]);
        }
        PlayerPrefs.Save();
    }

    // Remove saved level keys for this shop instance
    public void ClearSavedLevels()
    {
        if (currentLevels == null) return;

        // Delete saved keys
        for (int i = 0; i < currentLevels.Length; i++)
        {
            PlayerPrefs.DeleteKey(GetLevelKey(i));
            currentLevels[i] = 0; // reset runtime level
        }
        PlayerPrefs.Save();

        // Update UI to reflect cleared progress
        UpdateAllProgressBars();
    }

    private void LoadLevels()
    {
        if (currentLevels == null) return;
        for (int i = 0; i < currentLevels.Length; i++)
        {
            string key = GetLevelKey(i);
            if (PlayerPrefs.HasKey(key))
                currentLevels[i] = PlayerPrefs.GetInt(key);
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
                // Увеличиваем базовое здоровье на 5 и восстанавливаем до максимального
                ps.IncreaseMaxHealth(5f);
                ps.RestoreToMaxHealth();
                Debug.Log("[Shop] Базовое здоровье увеличено на 5 и восстановлено до максимума");
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