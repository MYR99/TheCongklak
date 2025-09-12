using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    
    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI playerOneScoreText;
    [SerializeField] private TextMeshProUGUI playerTwoScoreText;
    [SerializeField] private TextMeshProUGUI currentPlayerText;
    [SerializeField] private TextMeshProUGUI gameStatusText;
    
    [Header("Control Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button closeSettingsButton;
    
    [Header("Game End Panel")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button playAgainButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image currentPlayerIndicator;
    [SerializeField] private Color playerOneColor = Color.blue;
    [SerializeField] private Color playerTwoColor = Color.red;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupEventListeners();
        LoadSettings();
        HideGameEndPanel();
    }
    
    /// <summary>
    /// Setup UI event listeners
    /// </summary>
    private void SetupEventListeners()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
        
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(HideSettings);
        
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        
        if (soundToggle != null)
            soundToggle.onValueChanged.AddListener(ToggleSound);
        
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(SetDifficulty);
    }
    
    /// <summary>
    /// Update UI based on current game state
    /// </summary>
    public void UpdateGameState(BoardState boardState)
    {
        // Update scores
        if (playerOneScoreText != null)
            playerOneScoreText.text = $"Player 1: {boardState.playerOneScore}";
        
        if (playerTwoScoreText != null)
            playerTwoScoreText.text = $"Player 2: {boardState.playerTwoScore}";
        
        // Update current player indicator
        UpdateCurrentPlayerDisplay(boardState.currentPlayer);
        
        // Update game status
        UpdateGameStatus(boardState.gameStatus, boardState.winner);
    }
    
    /// <summary>
    /// Update current player display
    /// </summary>
    private void UpdateCurrentPlayerDisplay(int currentPlayer)
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = $"Current Turn: Player {currentPlayer}";
        }
        
        if (currentPlayerIndicator != null)
        {
            currentPlayerIndicator.color = currentPlayer == 1 ? playerOneColor : playerTwoColor;
        }
    }
    
    /// <summary>
    /// Update game status display
    /// </summary>
    private void UpdateGameStatus(GameStatus status, int winner)
    {
        switch (status)
        {
            case GameStatus.Playing:
                if (gameStatusText != null)
                    gameStatusText.text = "Game in Progress";
                HideGameEndPanel();
                break;
                
            case GameStatus.PlayerOneWins:
                ShowGameEnd("Player 1 Wins!");
                break;
                
            case GameStatus.PlayerTwoWins:
                ShowGameEnd("Player 2 Wins!");
                break;
                
            case GameStatus.Draw:
                ShowGameEnd("It's a Draw!");
                break;
        }
    }
    
    /// <summary>
    /// Show game end panel with result
    /// </summary>
    private void ShowGameEnd(string result)
    {
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(true);
        }
        
        if (winnerText != null)
        {
            winnerText.text = result;
        }
        
        if (gameStatusText != null)
        {
            gameStatusText.text = result;
        }
    }
    
    /// <summary>
    /// Hide game end panel
    /// </summary>
    private void HideGameEndPanel()
    {
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        
        HideGameEndPanel();
        HideSettings();
    }
    
    /// <summary>
    /// Show settings panel
    /// </summary>
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide settings panel
    /// </summary>
    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Exit the game
    /// </summary>
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameConfig.musicVolume = volume;
        }
        
        // AudioManager.Instance?.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameConfig.sfxVolume = volume;
        }
        
        // AudioManager.Instance?.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    /// <summary>
    /// Toggle sound on/off
    /// </summary>
    public void ToggleSound(bool enabled)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameConfig.soundEnabled = enabled;
        }
        
        // AudioManager.Instance?.SetSoundEnabled(enabled);
        PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
    }
    
    /// <summary>
    /// Set AI difficulty
    /// </summary>
    public void SetDifficulty(int difficultyIndex)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameConfig.aiDifficulty = (AILevel)difficultyIndex;
        }
        
        PlayerPrefs.SetInt("AIDifficulty", difficultyIndex);
    }
    
    /// <summary>
    /// Load saved settings
    /// </summary>
    private void LoadSettings()
    {
        // Load music volume
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;
        
        // Load SFX volume
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;
        
        // Load sound toggle
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        if (soundToggle != null)
            soundToggle.isOn = soundEnabled;
        
        // Load difficulty
        int difficulty = PlayerPrefs.GetInt("AIDifficulty", 1);
        if (difficultyDropdown != null)
            difficultyDropdown.value = difficulty;
    }
}