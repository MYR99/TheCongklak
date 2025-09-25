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

    // ========= MAIN BUTTONS =========
    public void OnRestartButton()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
            if (uiController != null) uiController.HideGameOver();

            Debug.Log("[UIButtonController] Restart pressed");
        }
    }

    public void OnExitButton()
    {
        Debug.Log("[UIButtonController] Exit pressed");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnSettingsButton()
    {
        if (uiController != null)
        {
            bool active = uiController.settingsPanel.activeSelf;
            uiController.settingsPanel.SetActive(!active);
            Debug.Log("[UIButtonController] Settings toggled " + (!active ? "ON" : "OFF"));
        }
    }

    // ========= SETTINGS CALLBACKS =========
    public void OnDifficultyChanged(int index)
    {
        if (uiController != null && uiController.difficultyDropdown != null)
        {
            Debug.Log($"[UIButtonController] Difficulty changed: {uiController.difficultyDropdown.options[index].text}");
        }
    }

    public void OnToggleSound(bool isOn)
    {
        if (uiController != null)
        {
            Debug.Log("[UIButtonController] Sound toggled: " + (isOn ? "ON" : "OFF"));
        }
    }
}