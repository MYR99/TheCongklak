using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIController uiController;

    private bool isFastForward = false;

    private void Awake()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (uiController == null) uiController = FindObjectOfType<UIController>();
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    public void OnFastForwardButton()
    {
        PlayClickSound();
        isFastForward = !isFastForward;
        Time.timeScale = isFastForward ? 2.0f : 1.0f;
    }
    
    public void OnPauseButton()
    {
        PlayClickSound();
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    public void OnResumeButton()
    {
        PlayClickSound();
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    public void OnRestartButton()
    {
        PlayClickSound();
        Time.timeScale = 1.0f; 
        isFastForward = false;
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    public void OnExitButton()
    {
        PlayClickSound();
        Time.timeScale = 1.0f;
        Debug.Log("[UIButtonController] Exit pressed");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnSettingsButton()
    {
        PlayClickSound();
        if (uiController == null || gameManager == null) return;
        bool settingsWillBeActive = !uiController.settingsPanel.activeSelf;
        uiController.settingsPanel.SetActive(settingsWillBeActive);
        if (gameManager.IsPaused)
        {
            gameManager.pausePanel.SetActive(!settingsWillBeActive);
        }
    }

    public void OnMainMenuButton()
    {
        PlayClickSound();
        Time.timeScale = 1.0f;
        
        SceneManager.LoadScene("MainMenu");
    }
}