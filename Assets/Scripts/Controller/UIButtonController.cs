using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIController uiController;

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
        Time.timeScale = 1f; 
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    public void OnExitButton()
    {
        PlayClickSound();
        Time.timeScale = 1f; 
        Debug.Log("[UIButtonController] Exit pressed");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // --- FUNGSI INI DIPERBARUI DENGAN LOGIKA BARU ---
    public void OnSettingsButton()
    {
        PlayClickSound();
        if (uiController == null || gameManager == null) return;

        // Tentukan apakah panel setting akan aktif atau nonaktif
        bool settingsWillBeActive = !uiController.settingsPanel.activeSelf;
        uiController.settingsPanel.SetActive(settingsWillBeActive);

        // Jika game sedang di-pause, kita juga harus mengatur PausePanel
        if (gameManager.IsPaused)
        {
            // Jika settings baru saja AKTIF, maka sembunyikan PausePanel.
            // Jika settings baru saja NONAKTIF, maka tampilkan lagi PausePanel.
            gameManager.pausePanel.SetActive(!settingsWillBeActive);
        }
    }
    // --------------------------------------------------
    
    public void OnDifficultyChanged(int index) { if (uiController != null && uiController.difficultyDropdown != null) { Debug.Log($"[UIButtonController] Difficulty changed: {uiController.difficultyDropdown.options[index].text}"); } }
    public void OnToggleSound(bool isOn) { if (uiController != null) { Debug.Log("[UIButtonController] Sound toggled: " + (isOn ? "ON" : "OFF")); } }
    public void OnMusicVolumeChanged(float value) { if (uiController != null) { Debug.Log("[UIButtonController] Music volume changed: " + value); } }
    public void OnSFXVolumeChanged(float value) { if (uiController != null) { Debug.Log("[UIButtonController] SFX volume changed: " + value); } }
}