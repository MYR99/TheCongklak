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
        if (config == null) { Debug.LogError("GameConfig is not assigned in the UIController Inspector!", this.gameObject); return; }
        musicVolumeSlider.value = config.musicVolume;
        sfxVolumeSlider.value = config.sfxVolume;
        soundToggle.isOn = config.soundEnabled;
        difficultyDropdown.value = (int)config.aiDifficulty;
        OnMusicVolumeChanged(config.musicVolume);
        OnSFXVolumeChanged(config.sfxVolume);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(config.musicVolume);
            AudioManager.Instance.SetSFXVolume(config.sfxVolume);
            AudioManager.Instance.ToggleSound(config.soundEnabled);
        }
    }
    
    public void UpdateScores(int playerOneScore, int playerTwoScore, int currentPlayer)
    {
        if (playerOneScoreTxt != null) playerOneScoreTxt.text = playerOneScore.ToString();
        if (playerTwoScoreTxt != null) playerTwoScoreTxt.text = playerTwoScore.ToString();
        if (currentPlayerTxt != null) { currentPlayerTxt.text = $"Giliran Pemain {currentPlayer}"; currentPlayerTxt.color = (currentPlayer == 1) ? Color.cyan : Color.magenta; }
        if (playerOneIndicator != null) playerOneIndicator.SetActive(currentPlayer == 1);
        if (playerTwoIndicator != null) playerTwoIndicator.SetActive(currentPlayer == 2);
    }
    public void UpdateGameStatus(string message) { if (gameStatusTxt != null) { gameStatusTxt.text = message; } }
    public void UpdateSeedCount(Hole hole, int count) { if (hole != null && hole.seedText != null) { hole.seedText.text = count.ToString(); } }
    public void ShowGameOver(string winnerMessage) { if (gameEndPanel != null) gameEndPanel.SetActive(true); if (winnerText != null) winnerText.text = winnerMessage; }
    public void HideGameOver() { if (gameEndPanel != null) gameEndPanel.SetActive(false); }
    
    public void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeLabel != null) musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100)}%";
        if (config != null) config.musicVolume = value;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%";
        if (config != null) config.sfxVolume = value;
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
    }
    
    public void OnSoundToggleChanged(bool isOn)
    {
        if (config != null) config.soundEnabled = isOn;
        if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound(isOn);
    }
}