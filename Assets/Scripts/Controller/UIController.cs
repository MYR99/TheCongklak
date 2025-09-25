using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private GameConfig config;

    [Header("Score & Turn UI")]
    public TMP_Text playerOneScoreTxt;
    public TMP_Text playerTwoScoreTxt;
    public TMP_Text gameStatusTxt;
    public TMP_Text currentPlayerTxt;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject gameEndPanel;

    [Header("Settings UI")]
    public Slider musicVolumeSlider;
    public TMP_Text musicVolumeLabel;
    public Slider sfxVolumeSlider;
    public TMP_Text sfxVolumeLabel;
    public Toggle soundToggle;
    public TMP_Dropdown difficultyDropdown;

    [Header("Game End UI")]
    public TMP_Text winnerText;

    [Header("Indicators")]
    public GameObject playerOneIndicator;
    public GameObject playerTwoIndicator;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameEndPanel != null) gameEndPanel.SetActive(false);

        LoadAndCheckSettings();
    }

    private void LoadAndCheckSettings()
    {
        if (config == null)
        {
            Debug.LogError("GameConfig is not assigned in the UIController Inspector!", this.gameObject);
            return;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = config.musicVolume;
            OnMusicVolumeChanged(config.musicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            sfxVolumeSlider.value = config.sfxVolume;
            OnSFXVolumeChanged(config.sfxVolume);
        }

        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            soundToggle.isOn = config.soundEnabled;
        }
    }

    // ========== UPDATE UI ==========
    public void UpdateScores(int p1Score, int p2Score)
    {
        if (playerOneScoreTxt != null) playerOneScoreTxt.text = p1Score.ToString();
        if (playerTwoScoreTxt != null) playerTwoScoreTxt.text = p2Score.ToString();
    }

    public void UpdateTurnUI(int currentPlayer)
    {
        if (gameStatusTxt != null)
        {
            gameStatusTxt.text = (currentPlayer == 1) ? "Player One's Turn" : "Player Two's Turn";
            gameStatusTxt.color = (currentPlayer == 1) ? Color.cyan : Color.magenta;
        }

        if (currentPlayerTxt != null)
        {
            currentPlayerTxt.text = (currentPlayer == 1) ? "Player One" : "Player Two";
        }

        if (playerOneIndicator != null) playerOneIndicator.SetActive(currentPlayer == 1);
        if (playerTwoIndicator != null) playerTwoIndicator.SetActive(currentPlayer == 2);
    }

    public void UpdateGameStatus(string message)
    {
        if (gameStatusTxt != null)
        {
            gameStatusTxt.text = message;
        }
    }

    public void UpdateSeedCount(Hole hole, int count)
    {
        if (hole != null && hole.seedText != null)
        {
            hole.seedText.text = count.ToString();
        }
    }

    public void ShowGameOver(string winnerMessage)
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(true);
        if (winnerText != null) winnerText.text = winnerMessage;
    }

    public void HideGameOver()
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(false);
    }

    // ========== SETTINGS CALLBACKS ==========
    public void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeLabel != null) musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100)}%";
        if (config != null) config.musicVolume = value;
        Debug.Log($"[UIController] Music volume set to: {value}");
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%";
        if (config != null) config.sfxVolume = value;
        Debug.Log($"[UIController] SFX volume set to: {value}");
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        if (config != null) config.soundEnabled = isOn;
    }
}