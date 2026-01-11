using UnityEngine;

public class PauseHandle : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;

    public void HandlePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel.SetActive(isPaused);
    }
}
