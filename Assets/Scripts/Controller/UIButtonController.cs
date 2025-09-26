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

    public void OnRestartButton()
    {
        PlayClickSound();
        if (gameManager != null)
        {
            gameManager.RestartGame();
            if (uiController != null) uiController.HideGameOver();
            Debug.Log("[UIButtonController] Restart pressed");
        }
    }

    public void OnExitButton()
    {
        PlayClickSound();
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
        if (uiController != null)
        {
            bool active = uiController.settingsPanel.activeSelf;
            uiController.settingsPanel.SetActive(!active);
            Debug.Log("[UIButtonController] Settings toggled " + (!active ? "ON" : "OFF"));
        }
    }

    public void OnDifficultyChanged(int index) { if (uiController != null && uiController.difficultyDropdown != null) { Debug.Log($"[UIButtonController] Difficulty changed: {uiController.difficultyDropdown.options[index].text}"); } }
    public void OnToggleSound(bool isOn) { if (uiController != null) { Debug.Log("[UIButtonController] Sound toggled: " + (isOn ? "ON" : "OFF")); } }
    public void OnMusicVolumeChanged(float value) { if (uiController != null) { Debug.Log("[UIButtonController] Music volume changed: " + value); } }
    public void OnSFXVolumeChanged(float value) { if (uiController != null) { Debug.Log("[UIButtonController] SFX volume changed: " + value); } }
}