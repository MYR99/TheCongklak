using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIController uiController;

    private bool isFastForwardActive = false;

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

    public void OnFastForwardButtonPressed()
    {
        PlayClickSound();

        isFastForwardActive = !isFastForwardActive;

        if (gameManager != null)
        {
            gameManager.SetFastForward(isFastForwardActive);
        }

        if (uiController != null)
        {
            uiController.ToggleFastForwardButtonColor(isFastForwardActive);
        }
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

        isFastForwardActive = false;
        if (uiController != null)
        {
            uiController.ToggleFastForwardButtonColor(isFastForwardActive);
        }

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
    }
}