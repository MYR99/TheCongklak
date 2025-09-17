using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Score UI")]
    public TMP_Text seedCountTxt;
    public TMP_Text playerOneScoreTxt;
    public TMP_Text playerTwoScoreTxt;
    public TMP_Text gameStatusTxt;

    [Header("Turn Indicator UI")]
    public TMP_Text currentPlayerTxt;
    public Image currentPlayerIndicator;

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
    public TMP_Text difficultyLabel;

    [Header("Game End UI")]
    public TMP_Text winnerText;

    private void Start()
    {
        // pastikan panel awal nonaktif
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameEndPanel != null) gameEndPanel.SetActive(false);

        // listener sliders, toggle, dropdown
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (soundToggle != null)
            soundToggle.onValueChanged.AddListener(OnSoundToggle);

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
    }

    // ✅ update skor dan total seeds
    public void UpdateBoardUI(BoardState boardState)
    {
        if (seedCountTxt != null)
        {
            int totalSeeds = 0;
            foreach (int s in boardState.playerOneHoles) totalSeeds += s;
            foreach (int s in boardState.playerTwoHoles) totalSeeds += s;
            seedCountTxt.text = "Seeds Left: " + totalSeeds;
        }

        if (playerOneScoreTxt != null)
            playerOneScoreTxt.text = "Player One: " + boardState.playerOneScore;

        if (playerTwoScoreTxt != null)
            playerTwoScoreTxt.text = "Player Two: " + boardState.playerTwoScore;
    }

    // ✅ update turn text dan indikator
    public void UpdateTurnIndicator(bool isPlayerOneTurn)
    {
        if (currentPlayerTxt != null)
            currentPlayerTxt.text = isPlayerOneTurn ? "Current: Player One" : "Current: Player Two";

        if (gameStatusTxt != null)
            gameStatusTxt.text = isPlayerOneTurn ? "Player One's Turn" : "Player Two's Turn";

        if (currentPlayerIndicator != null)
        {
            currentPlayerIndicator.color = isPlayerOneTurn ? Color.green : Color.red;
        }
    }

    // ✅ tampilkan game over
    public void ShowGameOver(string winnerMessage)
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(true);
        if (winnerText != null) winnerText.text = winnerMessage;
    }

    // ✅ buka/tutup settings
    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ✅ event handler UI
    private void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeLabel != null)
            musicVolumeLabel.text = "Music: " + Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeLabel != null)
            sfxVolumeLabel.text = "SFX: " + Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnSoundToggle(bool isOn)
    {
        Debug.Log("Sound " + (isOn ? "ON" : "OFF"));
    }

    private void OnDifficultyChanged(int index)
    {
        if (difficultyLabel != null)
            difficultyLabel.text = "Difficulty: " + difficultyDropdown.options[index].text;
    }
}