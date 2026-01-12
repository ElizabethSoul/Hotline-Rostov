using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    // Resume the game (useful if paused by setting Time.timeScale = 0)
    public void ContinueGame()
    {
        Time.timeScale = 1f;
    }

    // Quit application (works in build; stops playmode in editor)
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Load the main menu scene (scene name must match)
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    // Restart current scene
    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Load specific levels
    public void LoadLevel1() => LoadLevelByName("Level1");
    public void LoadLevel2() => LoadLevelByName("Level2");
    public void LoadLevel3() => LoadLevelByName("Level3");

    private void LoadLevelByName(string name)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }
}
