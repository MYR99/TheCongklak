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

    [Header("Speed Controls")]
    [SerializeField] private Button fastForwardButton;
    [SerializeField] private Color fastForwardActiveColor = Color.red;
    private Color fastForwardOriginalColor;

    private int lastP1Score = 0;
    private int lastP2Score = 0;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameEndPanel != null) gameEndPanel.SetActive(false);
        if (gameStatusTxt != null) gameStatusTxt.text = "";

        if (playerOneScoreTxt != null) playerOneScoreTxt.text = "0";
        if (playerTwoScoreTxt != null) playerTwoScoreTxt.text = "0";
        lastP1Score = 0;
        lastP2Score = 0;

        LoadSettingsFromConfig();
        if (fastForwardButton != null)
        {
            fastForwardOriginalColor = fastForwardButton.image.color;
        }
    }

    public void LoadSettingsFromConfig()
    {
        if (config == null) return;
        if (musicVolumeSlider != null) musicVolumeSlider.value = config.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = config.sfxVolume;
        if (soundToggle != null) soundToggle.isOn = config.soundEnabled;
        if (difficultyDropdown != null) difficultyDropdown.value = (int)config.aiDifficulty;
    }

    public void ShowGameOver(bool playerOneWon, bool isDraw)
    {
        if (gameEndPanel == null) return;
        gameEndPanel.SetActive(true);
        if (isDraw)
        {
            winnerImage.sprite = drawSprite;
        }
        else
        {
            winnerImage.sprite = playerOneWon ? winSprite : loseSprite;
        }
    }

    public void UpdateGameEndStats(int p1Score, int p2Score, int p1MaxCapture, int p2MaxCapture, int p1ExtraTurns, int p2ExtraTurns)
    {
        if (finalScoreText != null) finalScoreText.text = $"SKOR AKHIR\n{p1Score} - {p2Score}";
        if (statsText != null)
        {
            statsText.text = $"P1 Max Capture: {p1MaxCapture}\nP2 Max Capture: {p2MaxCapture}\nP1 Extra Turns: {p1ExtraTurns}\nP2 Extra Turns: {p2ExtraTurns}";
        }
    }

    public void UpdatePausePanelInfo(int p1Score, int p2Score, int playerTurn)
    {
        if (pausePanelScoreText != null) pausePanelScoreText.text = $"{p1Score} - {p2Score}";
        if (pausePanelTurnText != null) pausePanelTurnText.text = $"Giliran Pemain: {playerTurn}";
    }

    public void UpdateScores(int playerOneScore, int playerTwoScore, int currentPlayer)
    {
        if (playerOneScoreTxt != null)
        {
            if (playerOneScore != lastP1Score)
            {
                playerOneScoreTxt.text = playerOneScore.ToString();
                StopCoroutine(AnimateScoreText(playerOneScoreTxt));
                StartCoroutine(AnimateScoreText(playerOneScoreTxt));
                lastP1Score = playerOneScore;
            }
        }

        if (playerTwoScoreTxt != null)
        {
            if (playerTwoScore != lastP2Score)
            {
                playerTwoScoreTxt.text = playerTwoScore.ToString();
                StopCoroutine(AnimateScoreText(playerTwoScoreTxt));
                StartCoroutine(AnimateScoreText(playerTwoScoreTxt));
                lastP2Score = playerTwoScore;
            }
        }

        if (currentPlayerTxt != null) { currentPlayerTxt.text = $"Giliran Pemain {currentPlayer}"; currentPlayerTxt.color = (currentPlayer == 1) ? Color.cyan : Color.magenta; }
        if (playerOneIndicator != null) playerOneIndicator.SetActive(currentPlayer == 1);
        if (playerTwoIndicator != null) playerTwoIndicator.SetActive(currentPlayer == 2);
    }

    private IEnumerator AnimateScoreText(TMP_Text scoreText)
    {
        Vector3 originalScale = scoreText.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;
        float duration = 0.1f;

        float timer = 0;
        while (timer < duration)
        {
            scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        scoreText.transform.localScale = targetScale;

        timer = 0;
        while (timer < duration)
        {
            scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        scoreText.transform.localScale = originalScale;
    }

    public void UpdateGameStatus(string message) { if (gameStatusTxt != null) { gameStatusTxt.text = message; } }
    public void UpdateSeedCount(Hole hole, int count) { if (hole != null && hole.seedText != null) { hole.seedText.text = count.ToString(); } }
    public void HideGameOver() { if (gameEndPanel != null) gameEndPanel.SetActive(false); }

    public void OnMusicVolumeChanged(float value) { if (musicVolumeLabel != null) musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100)}%"; if (config != null) config.musicVolume = value; if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value); }
    public void OnSFXVolumeChanged(float value) { if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%"; if (config != null) config.sfxVolume = value; if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value); }
    public void OnSoundToggleChanged(bool isOn) { if (config != null) config.soundEnabled = isOn; if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound(isOn); }
    public void OnDifficultyChanged(int value) { if (config != null) config.aiDifficulty = (AILevel)value; }
    
    /// <summary>
    /// Mengubah warna tombol fast forward saat diaktifkan/dinonaktifkan
    /// </summary>
    public void ToggleFastForwardButtonColor(bool isActive)
    {
        if (fastForwardButton != null && fastForwardButton.image != null)
        {
            fastForwardButton.image.color = isActive ? fastForwardActiveColor : fastForwardOriginalColor;
        }
    }
}