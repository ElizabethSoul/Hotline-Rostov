using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Канвасы")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject levelsCanvas;
    [SerializeField] private GameObject optionsCanvas;

    public void OnStartButton()
    {
        levelsCanvas.SetActive(true);
    }
    public void OnOptionsButton()
    {
        optionsCanvas.SetActive(true);
    }

    public void OnLevel1Button()
    {
        LoadScene("Level1");
    }

    public void OnLevel2Button()
    {
        LoadScene("Level2");
    }

    public void OnLevel3Button()
    {
        LoadScene("Level3");
    }

    public void OnBackButton()  // кнопка "Назад" на экране выбора уровней
    {
        levelsCanvas.SetActive(false);
        optionsCanvas.SetActive(false);
    }

    public void OnQuitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}