
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField] Image easyImage;
    [SerializeField] Image normalImage;
    [SerializeField] Image hardImage;    
    void Start()
    {
        // Ensure SettingsManager exists in the scene
        if (SettingsManager.Instance == null)
        {
            var go = new GameObject("SettingsManager");
            go.AddComponent<SettingsManager>();
        }
    }

    // UI can call these methods (e.g., Button onClick)
    public void SetEasy()
    {
        SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Easy);
        easyImage.gameObject.SetActive(true);
        normalImage.gameObject.SetActive(false);
        hardImage.gameObject.SetActive(false);
    }

    public void SetNormal()
    {
        SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Normal);
        easyImage.gameObject.SetActive(false);
        normalImage.gameObject.SetActive(true);
        hardImage.gameObject.SetActive(false);
    }

    public void SetHard()
    {
        SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Hard);
        easyImage.gameObject.SetActive(false);
        normalImage.gameObject.SetActive(false);
        hardImage.gameObject.SetActive(true);
    }

    // Alternative: call with integer (0..2)
    public void SetDifficulty(int level)
    {
        var d = (SettingsManager.Difficulty)Mathf.Clamp(level, 0, 2);
        SettingsManager.Instance.SetDifficulty(d);
    }
}
