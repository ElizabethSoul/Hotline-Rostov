using UnityEngine;
using UnityEngine.SceneManagement;

public class Saves : MonoBehaviour
{
    // When true, after the next scene load the player's HP will be restored to max.
    public static bool restoreHPOnLoad = false;
    private const string Key_Coins = "save_coins";
    private const string Key_PlayerMaxHP = "save_player_maxhp";
    private const string Key_PlayerCurrentHP = "save_player_currenthp";
    private const string Key_PlayerFireCooldown = "save_player_firecooldown";
    private const string Key_PlayerDamage = "save_player_damage";

    // Сохранить текущее состояние в PlayerPrefs
    public void SaveAll()
    {
        PlayerPrefs.SetInt(Key_Coins, CurrencyManager.GetCoins());

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Здоровье игрока
            if (player.TryGetComponent<PlayerScript>(out var playerScript))
            {
                PlayerPrefs.SetFloat(Key_PlayerMaxHP, playerScript.GetMaxHealth());
                PlayerPrefs.SetFloat(Key_PlayerCurrentHP, playerScript.GetCurrentHealth());
            }

            // Атака/урон игрока
            if (player.TryGetComponent<PlayerAttack>(out var playerAttack))
            {
                PlayerPrefs.SetFloat(Key_PlayerFireCooldown, playerAttack.GetFireCooldown());
                PlayerPrefs.SetInt(Key_PlayerDamage, playerAttack.GetDamage());
            }
        }

        PlayerPrefs.Save();
        Debug.Log("[Saves] Игра сохранена в PlayerPrefs");
    }

    // Загрузить сохранённые значения (если они существуют)
    public void LoadAll()
    {
        // Монеты
        if (PlayerPrefs.HasKey(Key_Coins))
        {
            CurrencyManager.SetCoins(PlayerPrefs.GetInt(Key_Coins));
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Здоровье игрока
            if (player.TryGetComponent<PlayerScript>(out var playerScript))
            {
                if (PlayerPrefs.HasKey(Key_PlayerMaxHP))
                {
                    playerScript.SetMaxHealth(PlayerPrefs.GetFloat(Key_PlayerMaxHP));
                }
                if (PlayerPrefs.HasKey(Key_PlayerCurrentHP))
                {
                    playerScript.SetCurrentHealth(PlayerPrefs.GetFloat(Key_PlayerCurrentHP));
                }
            }

            // Атака/урон игрока
            if (player.TryGetComponent<PlayerAttack>(out var playerAttack))
            {
                if (PlayerPrefs.HasKey(Key_PlayerFireCooldown))
                {
                    playerAttack.SetFireCooldown(PlayerPrefs.GetFloat(Key_PlayerFireCooldown));
                }
                if (PlayerPrefs.HasKey(Key_PlayerDamage))
                {
                    playerAttack.SetDamage(PlayerPrefs.GetInt(Key_PlayerDamage));
                }
            }
        }

        Debug.Log("[Saves] Игра загружена из PlayerPrefs");
    }

    // Очистить все сохранения
    public void ClearSaves()
    {
            // Remove all PlayerPrefs keys to ensure no leftover keys (including shop_* variants) remain.
            PlayerPrefs.DeleteAll();

            // Reset runtime coin count so UI updates immediately in menus/scenes.
            try
            {
                CurrencyManager.SetCoins(0);
            }
            catch
            {
                // If CurrencyManager is not available in this context, ignore.
            }

            // Clear runtime shop levels and update their UI. Use FindObjectsOfTypeAll to catch prefabs and instances.
            var shops = Resources.FindObjectsOfTypeAll<Shop>();
            foreach (var s in shops)
            {
                if (s != null)
                    s.ClearSavedLevels();
            }

            PlayerPrefs.Save();
            Debug.Log("[Saves] Все сохранённые данные полностью очищены (PlayerPrefs и прогресс магазинов)");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadAll();

        if (restoreHPOnLoad)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.TryGetComponent<PlayerScript>(out var playerScript))
            {
                playerScript.RestoreToMaxHealth();
            }
            restoreHPOnLoad = false;
        }
    }
}