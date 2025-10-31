using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialText;   
    [SerializeField] private Button nextButton;        
    [SerializeField] private GameObject handCursor;

    [Header("Game References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Hole[] playerOneHoles;
    [SerializeField] private GameConfig gameConfig;

    // State untuk menunggu pemain menekan "Lanjut"
    public bool isReadyForNextStep { get; private set; } = true;
    private int tutorialHoleIndex = 3;

    private void OnEnable()
    {
        GameManager.OnTutorialSowContinue += HandleSowContinue;
        GameManager.OnTutorialCapture += HandleCapture;
        GameManager.OnTutorialExtraTurn += HandleExtraTurn;
        GameManager.OnTutorialTurnEnd += HandleTurnEnd;
    }

    private void OnDisable()
    {
        GameManager.OnTutorialSowContinue -= HandleSowContinue;
        GameManager.OnTutorialCapture -= HandleCapture;
        GameManager.OnTutorialExtraTurn -= HandleExtraTurn;
        GameManager.OnTutorialTurnEnd -= HandleTurnEnd;
    }

    private void Start()
    {
        if (gameConfig == null)
        {
            gameConfig = gameManager.config;
        }
        
        tutorialPanel.SetActive(false);
        if (handCursor != null) handCursor.SetActive(false);
        nextButton.onClick.AddListener(OnNextClicked);
    }

    public IEnumerator StartTutorialSequence()
    {
        bool isIndonesian = gameConfig.currentLanguage == Language.Indonesian;

        // Langkah 1: Sambutan
        string welcomeMsg = isIndonesian ? 
            "Selamat datang di The Congklak!\nIni adalah tutorial singkat mode PvE." : 
            "Welcome to The Congklak!\nThis is a short PvE tutorial.";
        yield return ShowMessage(welcomeMsg);
        
        // Langkah 2: Jelaskan Tujuan
       string goalMsg = isIndonesian ? 
            "Tujuanmu adalah mengumpulkan biji sebanyak-banyaknya di 'Lumbung' (lubang besar) di sebelah kiri." : 
            "Your goal is to collect as many seeds as possible in your 'Storehouse' (the big hole) on your left.";
        yield return ShowMessage(goalMsg);
        
        // Langkah 3: Tunjuk Lubang
       string instructMsg = isIndonesian ? 
            "Sisi Anda adalah 7 lubang di baris bawah.\n\nCoba klik lubang yang kami tunjuk untuk mulai 'Menabur'." : 
            "Your side is the 7 holes on the bottom row.\n\nTry clicking the indicated hole to start 'Sowing'.";
        ShowInstruction(instructMsg);

        PointAtHole(playerOneHoles[tutorialHoleIndex]);
        
        gameManager.SetGameActive(true);
    }

    // Menampilkan pesan dan menunggu pemain menekan "Lanjut"
    private IEnumerator ShowMessage(string message)
    {
        isReadyForNextStep = false;
        tutorialPanel.SetActive(true);
        tutorialText.text = message;
        nextButton.gameObject.SetActive(true);
        if (handCursor != null) handCursor.SetActive(false);

        // Tunggu sampai tombol "Lanjut" ditekan
        yield return new WaitUntil(() => isReadyForNextStep);
    }

    // Menampilkan instruksi (tanpa tombol "Lanjut")
    private void ShowInstruction(string message)
    {
        isReadyForNextStep = false;
        tutorialPanel.SetActive(true);
        tutorialText.text = message;
        nextButton.gameObject.SetActive(false);
    }

    private void OnNextClicked()
    {
        isReadyForNextStep = true;
        tutorialPanel.SetActive(false);
    }

    private void PointAtHole(Hole hole)
    {
        if (handCursor == null) return;

        RectTransform cursorRect = handCursor.GetComponent<RectTransform>();
        if (cursorRect == null)
        {
            Debug.LogError("HandCursor tidak memiliki RectTransform! Pastikan ini adalah UI Image.");
            return;
        }

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(hole.transform.position);

        cursorRect.position = screenPoint + new Vector2(0, 50f);
        handCursor.SetActive(true);
    }

    private void HandleSowContinue(Hole hole)
    {
        bool isIndonesian = gameConfig.currentLanguage == Language.Indonesian;
        string msg = isIndonesian ?
            $"Biji terakhir mendarat di lubang berisi.\nAnda mengambil semua biji ({hole.GetSeedCount()}) dan LANJUT menabur!" :
            $"Last seed landed in an occupied hole.\nYou take all ({hole.GetSeedCount()}) seeds and CONTINUE sowing!";
        StartCoroutine(ShowMessage(msg));
    }

    private void HandleCapture(Hole hole)
    {
        bool isIndonesian = gameConfig.currentLanguage == Language.Indonesian;
        string msg = isIndonesian ?
            $"Biji terakhir mendarat di lubang KOSONG di sisimu.\nIni disebut 'Menembak'!\nAnda mengambil semua biji di lubang lawan seberang." :
            $"Last seed landed in an EMPTY hole on your side.\nThis is a 'Capture' (or 'Nembak')!\nYou capture all seeds in the opposite hole.";
        StartCoroutine(ShowMessage(msg));
    }

    private void HandleExtraTurn()
    {
        bool isIndonesian = gameConfig.currentLanguage == Language.Indonesian;
        string msg = isIndonesian ?
            "Biji terakhir mendarat di LUMBUNG Anda!\nAnda dapat 'Giliran Tambahan' dan boleh main lagi." :
            "Last seed landed in your STOREHOUSE!\nYou get an 'Extra Turn' and can play again.";
        StartCoroutine(ShowMessage(msg));
    }

    private void HandleTurnEnd()
    {
        // Giliran Selesai
        StartCoroutine(EndTutorialSequence());
    }
    
    private IEnumerator EndTutorialSequence()
    {
        bool isIndonesian = gameConfig.currentLanguage == Language.Indonesian;
        string msg = isIndonesian ?
            "Giliran Anda berakhir. Sekarang giliran AI.\nTutorial Selesai! Selamat bermain!" :
            "Your turn ended. It is now the AI's turn.\nTutorial Complete! Have fun!";
        
        yield return ShowMessage(msg);
    }
}