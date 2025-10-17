using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameConfig gameConfig;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject aiDifficultyPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI Elements")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeLabel;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeLabel;

    private void Start()
    {

        mainMenuPanel.SetActive(true);
        modeSelectPanel.SetActive(false);
        aiDifficultyPanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic();
        }
        
        LoadSettingsToUI();
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    public void ShowModeSelectPanel()
    {
        PlayClickSound();
        mainMenuPanel.SetActive(false);
        modeSelectPanel.SetActive(true);
    }

    public void ShowAIDifficultyPanel()
    {
        PlayClickSound();
        modeSelectPanel.SetActive(false);
        aiDifficultyPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        PlayClickSound();

        modeSelectPanel.SetActive(false);
        aiDifficultyPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowSettingsPanel()
    {
        PlayClickSound();
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void SelectGameMode(int mode) 
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig belum di-assign di MainMenuController!");
            return;
        }

        if (mode == 0) 
        {
            PlayClickSound();
            gameConfig.gameMode = GameMode.PlayerVsPlayer;
            StartGame();
        }
        else 
        {
            gameConfig.gameMode = GameMode.PlayerVsAI;
            ShowAIDifficultyPanel();
        }
    }
    
    public void SelectDifficulty(int level)
    {
        PlayClickSound();
        if (gameConfig != null)
        {
            gameConfig.aiDifficulty = (AILevel)level;
            StartGame();
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Keluar dari permainan...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void LoadSettingsToUI()
    {
        if (gameConfig == null) return;
        
        musicVolumeSlider.value = gameConfig.musicVolume;
        sfxVolumeSlider.value = gameConfig.sfxVolume;
        OnMusicVolumeChanged(gameConfig.musicVolume);
        OnSFXVolumeChanged(gameConfig.sfxVolume);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (gameConfig != null) gameConfig.musicVolume = value;
        if (musicVolumeLabel != null) musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100)}%";
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (gameConfig != null) gameConfig.sfxVolume = value;
        if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%";
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
    }
}