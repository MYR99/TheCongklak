using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Translated Text")]
    [SerializeField] private TMP_Text settingsPanelTitle;
    [SerializeField] private TMP_Text languageLabel;
    [SerializeField] private TMP_Text musicLabel;
    [SerializeField] private TMP_Text sfxLabel;
    [SerializeField] private TMP_Text soundLabel;
    [SerializeField] private TMP_Text difficultyLabel;
    
    [Header("Game End UI")]
    public Image winnerImage;
    public Sprite winSprite;
    public Sprite loseSprite;
    public Sprite drawSprite;
    public TMP_Text finalScoreText;
    public TMP_Text statsText;

    [Header("Pause Panel UI")]
    public TMP_Text pausePanelScoreText;
    public TMP_Text pausePanelTurnText;

    [Header("Indicators")]
    public GameObject playerOneIndicator;
    public GameObject playerTwoIndicator;
    public Toggle fastForwardButton; // Referensi untuk Toggle

    [Header("Tutorial Panels")]
    [Tooltip("Panel 'induk' yang berisi semua halaman tutorial pengenalan")]
    public GameObject introTutorialPanelContainer; 
    [Tooltip("Array dari setiap halaman/panel tutorial pengenalan, urutkan dari 0")]
    public GameObject[] introTutorialPanels; 
    [Space(10)]
    [Tooltip("Panel 'induk' untuk tips kontekstual (Capture, Extra Turn)")]
    public GameObject contextualTutorialPanel; 
    [Tooltip("Teks Judul untuk panel kontekstual")]
    public TMP_Text contextualTutorialTitle;
    [Tooltip("Teks Isi/Body untuk panel kontekstual")]
    public TMP_Text contextualTutorialBody;

    private int currentIntroPanelIndex = 0;


    private void Start()
    {
        // Pastikan panel tutorial tersembunyi saat game dimulai
        if (introTutorialPanelContainer != null) introTutorialPanelContainer.SetActive(false);
        if (contextualTutorialPanel != null) contextualTutorialPanel.SetActive(false);

        // Panggil fungsi yang ada dari skrip Anda
        LoadSettingsToUI();
    }
    
    public void UpdateScores(int p1Score, int p2Score, int currentPlayer)
    {
        if (playerOneScoreTxt != null) playerOneScoreTxt.text = p1Score.ToString();
        if (playerTwoScoreTxt != null) playerTwoScoreTxt.text = p2Score.ToString();
        
        if (playerOneIndicator != null) playerOneIndicator.SetActive(currentPlayer == 1);
        if (playerTwoIndicator != null) playerTwoIndicator.SetActive(currentPlayer == 2);
    }

    public void UpdateGameStatus(string message, int player = 0)
    {
        if (gameStatusTxt != null) gameStatusTxt.text = message;
        if (currentPlayerTxt != null)
        {
            if (player == 1) currentPlayerTxt.text = (config.currentLanguage == Language.Indonesian) ? "Giliran Player 1" : "Player 1 Turn";
            else if (player == 2) currentPlayerTxt.text = (config.currentLanguage == Language.Indonesian) ? "Giliran Player 2" : "Player 2 Turn";
            else currentPlayerTxt.text = "";
        }
    }
    
    public void ShowGameOver(string winner, int p1Score, int p2Score, string stats)
    {
        if (gameEndPanel == null) return;
        gameEndPanel.SetActive(true);

        if (finalScoreText != null) finalScoreText.text = $"{p1Score} - {p2Score}";
        if (statsText != null) statsText.text = stats;

        if (winnerImage != null)
        {
            if (winner == "P1") winnerImage.sprite = winSprite;
            else if (winner == "P2") winnerImage.sprite = loseSprite;
            else winnerImage.sprite = drawSprite;
        }
    }
    
    public void UpdatePausePanelInfo(int p1Score, int p2Score, int currentPlayer)
    {
        if (pausePanelScoreText != null) pausePanelScoreText.text = $"{p1Score} - {p2Score}";
        bool isIndonesian = config.currentLanguage == Language.Indonesian;
        if (pausePanelTurnText != null) pausePanelTurnText.text = (isIndonesian ? "Giliran: Player " : "Current Turn: Player ") + currentPlayer;
    }
    
    private void LoadSettingsToUI()
    {
        if (config == null) return;

        if (musicVolumeSlider != null) musicVolumeSlider.value = config.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = config.sfxVolume;
        if (soundToggle != null) soundToggle.isOn = config.soundEnabled;
        if (difficultyDropdown != null) difficultyDropdown.value = (int)config.aiDifficulty;
        if (languageDropdown != null) languageDropdown.value = (int)config.currentLanguage;

        UpdateAllText(); 
    }
    
    public void UpdateAllText()
    {
        if (config == null) return;
        bool isIndonesian = config.currentLanguage == Language.Indonesian;

        if (settingsPanelTitle != null) settingsPanelTitle.text = isIndonesian ? "Pengaturan" : "Settings";
        if (languageLabel != null) languageLabel.text = isIndonesian ? "Bahasa" : "Language";

        if (musicVolumeLabel != null) musicVolumeLabel.text = $"{(isIndonesian ? "Musik" : "Music")}: {Mathf.RoundToInt(config.musicVolume * 100)}%";
        if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(config.sfxVolume * 100)}%";
    }
    
    public void OnLanguageChanged(int value)
    {
        if (config != null) config.currentLanguage = (Language)value;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        UpdateAllText();
    }

    public void UpdateHoleSeedText(Hole hole, int count) { if (hole != null && hole.seedText != null) { hole.seedText.text = count.ToString(); } }
    public void HideGameOver() { if (gameEndPanel != null) gameEndPanel.SetActive(false); }
    public void OnMusicVolumeChanged(float value) { if (musicVolumeLabel != null) musicVolumeLabel.text = $"{(config.currentLanguage == Language.Indonesian ? "Musik" : "Music")}: {Mathf.RoundToInt(value * 100)}%"; if (config != null) config.musicVolume = value; if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value); }
    public void OnSFXVolumeChanged(float value) { if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%"; if (config != null) config.sfxVolume = value; if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value); }
    public void OnSoundToggleChanged(bool isOn) { if (config != null) config.soundEnabled = isOn; if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound(isOn); }
    public void OnDifficultyChanged(int value) { if (config != null) config.aiDifficulty = (AILevel)value; }
    
    public void ToggleFastForwardButtonColor(bool isActive)
    {
        if (fastForwardButton == null) return;
        Image toggleImage = fastForwardButton.targetGraphic as Image;
        if (toggleImage != null)
        {
            toggleImage.color = isActive ? Color.green : Color.white; 
        }
    }

    /// <summary>
    /// Dipanggil oleh GameManager untuk memulai tutorial pengenalan
    /// </summary>
    public void ShowIntroTutorial()
    {
        currentIntroPanelIndex = 0;
        introTutorialPanelContainer.SetActive(true);
        
        // Tampilkan hanya panel pertama
        for (int i = 0; i < introTutorialPanels.Length; i++)
        {
            introTutorialPanels[i].SetActive(i == 0);
        }
    }

    /// <summary>
    /// Dipanggil oleh tombol "Next" / "Lanjut" di panel tutorial pengenalan
    /// </summary>
    public void OnIntroTutorialNext()
    {
        // Sembunyikan panel saat ini
        if (currentIntroPanelIndex < introTutorialPanels.Length)
        {
            introTutorialPanels[currentIntroPanelIndex].SetActive(false);
        }

        currentIntroPanelIndex++;

        if (currentIntroPanelIndex < introTutorialPanels.Length)
        {
            // Tampilkan panel berikutnya
            introTutorialPanels[currentIntroPanelIndex].SetActive(true);
        }
        else
        {
            // Selesai, beri tahu GameManager
            GameManager.Instance.CompleteIntroTutorial();
        }
    }

    /// <summary>
    /// Dipanggil oleh GameManager saat panel terakhir ditutup
    /// </summary>
    public void HideIntroTutorial()
    {
        introTutorialPanelContainer.SetActive(false);
    }

    /// <summary>
    /// Dipanggil oleh GameManager saat game di-pause untuk memberi tips
    /// </summary>
    public void ShowContextualTutorial(TutorialType type)
    {
        bool isIndonesian = (config != null && config.currentLanguage == Language.Indonesian);
        string title = "";
        string body = "";

        switch (type)
        {
            case TutorialType.ExtraTurn:
                title = isIndonesian ? "GILIRAN TAMBAHAN!" : "EXTRA TURN!";
                body = isIndonesian ? 
                    "Biji terakhir Anda mendarat di Lumbung (Storehouse). Anda dapat giliran bermain lagi!" : 
                    "Your last seed landed in your Storehouse. You get an extra turn!";
                break;
            case TutorialType.Capture:
                title = isIndonesian ? "MENEMBAK (CAPTURE)!" : "CAPTURE!";
                body = isIndonesian ? 
                    "Biji terakhir Anda mendarat di lubang kosong di sisi Anda. Anda mengambil semua biji di lubang seberang!" : 
                    "Your last seed landed in an empty hole on your side. You capture all seeds in the opposite hole!";
                break;
            case TutorialType.SowContinue:
                title = isIndonesian ? "LANJUT MENABUR" : "CONTINUE SOWING";
                body = isIndonesian ? 
                    "Biji terakhir mendarat di lubang yang ada isinya. Anda mengambil semua biji dan lanjut menabur." : 
                    "The last seed landed in a hole with seeds. You pick up all seeds and continue sowing.";
                break;
        }

        contextualTutorialTitle.text = title;
        contextualTutorialBody.text = body;
        contextualTutorialPanel.SetActive(true);
    }

    /// <summary>
    /// Dipanggil oleh tombol "Close" / "Lanjut" di panel kontekstual
    /// </summary>
    public void HideContextualTutorial()
    {
        contextualTutorialPanel.SetActive(false);
        // Beri tahu GameManager untuk melanjutkan game
        GameManager.Instance.ResumeFromTutorial();
    }
}