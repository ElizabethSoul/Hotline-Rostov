using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    [SerializeField]
    private Difficulty difficulty = Difficulty.Normal;

    private const string PREF_KEY = "game_difficulty";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            difficulty = (Difficulty)PlayerPrefs.GetInt(PREF_KEY);
        }
    }

    public Difficulty CurrentDifficulty => difficulty;

    public void SetDifficulty(Difficulty d)
    {
        difficulty = d;
        PlayerPrefs.SetInt(PREF_KEY, (int)d);
        PlayerPrefs.Save();
    }

    public int GetEnemyHPBonus()
    {
        return ((int)difficulty) * 20;
    }

    public int GetEnemyAttackBonus()
    {
        return ((int)difficulty) * 10;
    }

    public int GetEnemyHP(int baseHP)
    {
        return baseHP + GetEnemyHPBonus();
    }

    public int GetEnemyAttack(int baseAttack)
    {
        return baseAttack + GetEnemyAttackBonus();
    }
}
