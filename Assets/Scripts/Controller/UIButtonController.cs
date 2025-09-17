using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    // ✅ restart game
    public void OnRestartGame()
    {
        Debug.Log("Restart button clicked!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ✅ buka settings
    public void OnOpenSettings(GameObject settingsPanel)
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // ✅ tutup settings
    public void OnCloseSettings(GameObject settingsPanel)
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ✅ exit game
    public void OnExitGame()
    {
        Debug.Log("Exit button clicked!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit(); 
#endif
    }

    // ✅ play again dari game end panel
    public void OnPlayAgain()
    {
        Debug.Log("Play Again button clicked!");
        OnRestartGame();
    }
}