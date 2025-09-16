using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Dropdown aiDifficultyDropdown;

    [Header("References")]
    [SerializeField] private GameConfig gameConfig; // Optional: will auto-load if null

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Try auto-load GameConfig if not assigned in Inspector
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfig>("GameConfig");
            if (gameConfig == null)
            {
                Debug.LogError("⚠ No GameConfig found! Please create one via Assets → Create → Congklak → Game Config, and place it in a Resources folder.");
            }
        }
    }

    private void Start()
    {
        if (gameConfig != null)
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = gameConfig.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener((value) =>
            {
                gameConfig.musicVolume = value;
            });
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = gameConfig.sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener((value) =>
            {
                gameConfig.sfxVolume = value;
            });
        }

        if (soundToggle != null)
        {
            soundToggle.isOn = gameConfig.soundEnabled;
            soundToggle.onValueChanged.AddListener((enabled) =>
            {
                gameConfig.soundEnabled = enabled;
            });
        }

        if (aiDifficultyDropdown != null)
        {
            aiDifficultyDropdown.value = (int)gameConfig.aiDifficulty;
            aiDifficultyDropdown.onValueChanged.AddListener((index) =>
            {
                gameConfig.aiDifficulty = (AILevel)index;
            });
        }
    }

    public void UpdateGameState(BoardState boardState)
    {
        if (gameStatusText == null) return;

        switch (boardState.gameStatus)
        {
            case GameStatus.Playing:
                gameStatusText.text = "Player " + boardState.currentPlayer + "'s Turn";
                break;
            case GameStatus.PlayerOneWins:
                gameStatusText.text = "Player 1 Wins!";
                break;
            case GameStatus.PlayerTwoWins:
                gameStatusText.text = "Player 2 Wins!";
                break;
            case GameStatus.Draw:
                gameStatusText.text = "It's a Draw!";
                break;
        }
    }
}