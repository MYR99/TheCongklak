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
    // HandCursor dihapus

    [Header("Game References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private Hole[] playerOneHoles; // (PENTING: Pastikan ini baris bawah, 0=kanan, 6=kiri)
    [SerializeField] private Hole[] playerTwoHoles;
    [SerializeField] private Hole playerOneStorehouse;
    [SerializeField] private Hole playerTwoStorehouse;
    [SerializeField] private UIController uiController;

    public bool isReadyForNextStep { get; private set; } = true;
    private bool isIndonesian = true;

    // GetCurrentTutorialHole() dipindah ke GameManager

    #region Event Subscription
    private void OnEnable()
    {
        // Hapus OnTutorialScoreUpdate untuk perbaiki 'freeze'
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
    #endregion

    private void Start()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (uiController == null) uiController = FindObjectOfType<UIController>();
        if (gameConfig == null && gameManager != null) gameConfig = gameManager.config;

        if (gameConfig != null)
        {
            isIndonesian = gameConfig.currentLanguage == Language.Indonesian;
        }
        
        tutorialPanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public IEnumerator StartTutorialSequence()
    {
        // =================================================================
        // STEP 1: SELAMAT DATANG (Hanya Teks)
        // =================================================================
        yield return ShowMessage(isIndonesian ? "Selamat Datang di The Congklak!" : "Welcome to The Congklak!");
        yield return ShowMessage(isIndonesian ? "Congklak (atau 'Dakon') adalah permainan papan tradisional dari Asia Tenggara. Ini adalah versi Indonesia-nya." : "Congklak (or 'Dakon') is a traditional board game from Southeast Asia. This is the Indonesian version.");
        yield return ShowMessage(isIndonesian ? "Sisi Anda adalah 7 'Lubang' di baris bawah. Tujuannya adalah mengumpulkan biji sebanyak-banyaknya di 'Lumbung' (lubang besar) di sebelah KIRI Anda." : "Your side is the 7 'Holes' on the bottom row. Your goal is to collect seeds in your 'Storehouse' (the big hole) on your LEFT.");

        // =================================================================
        // STEP 2: LOGIKA GAME (SESUAI ALUR ANDA)
        // =================================================================

        // ---------------------------------
        // Skenario 1 (Alur Panjang Awal)
        // ---------------------------------
        yield return ShowMessage(isIndonesian ?
            "Mari kita mulai. Papan sudah terisi 7 biji per lubang, seperti permainan asli." :
            "Let's begin. The board is full (7 seeds per hole) just like a real game.");
        
        // Papan sudah di-setup oleh GameManager.Start()
        uiController.UpdateScores(0, 0, 1);

        // --- PERBAIKAN: Lubang ke-4 dari KANAN adalah index 3 ---
        // (Sesuai permintaan Anda "lubang keemp[at dari kanan")
        Hole targetHole = playerOneHoles[3]; // P1_3
        
        // Minta GameManager menyalakan indikator di lubang itu
        gameManager.SetTutorialHole(targetHole); 
        
        string firstMoveMsg = isIndonesian ?
            "Untuk memulai, klik lubang yang menyala (lubang ke-4 dari kanan)." :
            "To begin, please click the highlighted hole (the 4th hole from the right).";
        
        yield return ShowInstruction(firstMoveMsg); // Menunggu klik
        
        // PAUSE: Menunggu event pertama (Sow Continue)
        // (Klik P1_3 [7 biji] -> P1_4 -> P1_5 -> P1_6 -> P1_Store -> P2_0 -> P2_1 -> P2_2 [penuh] -> Sow Continue)
        yield return new WaitUntil(() => isReadyForNextStep); 

        // ---------------------------------
        // Skenario 2 (Extra Turn)
        // ---------------------------------
        yield return ShowMessage(isIndonesian ? "Bagus! Sekarang, mari kita lihat 'Giliran Tambahan'." : "Great! Now, let's look at 'Extra Turns'.");
        
        // Setup papan untuk skenario ini
        ClearBoardForTutorial();
        playerOneHoles[5].SetSeeds(1); // 1 biji di P1_5
        uiController.UpdateScores(0, 0, 1);
        gameManager.SetTutorialHole(playerOneHoles[5]);

        string extraTurnMsg = isIndonesian ?
            "Jika biji terakhir mendarat di LUMBUNG Anda, skor Anda bertambah dan Anda dapat giliran tambahan!\n\nKlik lubang yang menyala." :
            "If your last seed lands in your STOREHOUSE, your score increases and you get an extra turn!\n\nPlease click the highlighted hole.";

        yield return ShowInstruction(extraTurnMsg); // Menunggu klik
        yield return new WaitUntil(() => isReadyForNextStep); // Menunggu HandleExtraTurn

        // ---------------------------------
        // Skenario 3 (Capture / 'Menembak')
        // ---------------------------------
        yield return ShowMessage(isIndonesian ? "Terakhir, aturan terpenting: 'Menembak' (Capture)." : "Finally, the most important rule: 'Capturing'.");
        
        ClearBoardForTutorial();
        playerOneHoles[2].SetSeeds(1); // 1 biji di P1_2
        Hole oppositeHole = gameManager.GetOppositeHole(playerOneHoles[2]);
        if (oppositeHole != null) oppositeHole.SetSeeds(5);
        uiController.UpdateScores(0, 0, 1);
        gameManager.SetTutorialHole(playerOneHoles[2]);

        string captureMsg = isIndonesian ?
            "Jika biji terakhir mendarat di lubang KOSONG di sisi Anda, DAN lubang lawan berisi biji... Anda 'Menembak'!\n\nKlik lubang yang menyala." :
            "If your last seed lands in an EMPTY hole on your side, AND the opponent's hole has seeds... You 'Capture'!\n\nPlease click the highlighted hole.";
        
        yield return ShowInstruction(captureMsg); // Menunggu klik
        yield return new WaitUntil(() => isReadyForNextStep); // Menunggu HandleCapture
        
        // ---------------------------------
        // Skenario 4 (Turn End di Sisi Lawan)
        // ---------------------------------
        yield return ShowMessage(isIndonesian ? "Dan jika biji terakhir mendarat di lubang KOSONG di sisi LAWAN..." : "And if the last seed lands in an EMPTY hole on the OPPONENT'S side...");

        ClearBoardForTutorial();
        playerOneHoles[6].SetSeeds(2); // 2 biji di P1_6
        uiController.UpdateScores(0, 0, 1);
        gameManager.SetTutorialHole(playerOneHoles[6]);
        
        string turnEndMsg = isIndonesian ?
            "...giliran Anda berakhir.\n\nKlik lubang yang menyala." :
            "...your turn ends.\n\nPlease click the highlighted hole.";

        yield return ShowInstruction(turnEndMsg); // Menunggu klik
        yield return new WaitUntil(() => isReadyForNextStep); // Menunggu HandleTurnEnd

        // =================================================================
        // STEP 3: TUTORIAL SELESAI
        // =================================================================
        yield return ShowMessage(isIndonesian ? "Tutorial Selesai! Permainan akan dimulai ulang secara normal. Selamat bermain!" : "Tutorial Complete! The game will now restart normally. Have fun!");

        gameManager.EndTutorial();
    }

    // =================================================================
    // HELPER FUNCTIONS
    // =================================================================

    // Menampilkan pesan dengan tombol "Next"
    private IEnumerator ShowMessage(string message)
    {
        gameManager.SetGameActive(false); // PAUSE
        isReadyForNextStep = false;       
        
        tutorialPanel.SetActive(true);
        tutorialText.text = message;
        nextButton.gameObject.SetActive(true); 
        
        yield return new WaitUntil(() => isReadyForNextStep);
        
        tutorialPanel.SetActive(false);
    }

    // Menampilkan instruksi (TANPA tombol "Next") dan menunggu klik
    private IEnumerator ShowInstruction(string message)
    {
        gameManager.SetGameActive(true); // Izinkan klik
        isReadyForNextStep = false;      // Tutorial 'pause' menunggu event
        
        tutorialPanel.SetActive(true);
        tutorialText.text = message;
        nextButton.gameObject.SetActive(false); // Sembunyikan 'Next'
        
        // GameManager sudah menyalakan indikator
        
        // Kita tidak menunggu di sini. Kita biarkan 'StartTutorialSequence' menunggu
        // event selanjutnya.
        yield return null;
    }

    public void OnSeedLanded(Hole landedHole)
    {
        // Dikosongkan
    }

    private void OnNextButtonClicked()
    {
        isReadyForNextStep = true;
    }

    // PointAtHole() DIHAPUS

    private void ClearBoardForTutorial()
    {
        // Kosongkan semua lubang untuk skenario kustom
        foreach (var hole in playerOneHoles) hole.ClearSeeds();
        foreach (var hole in playerTwoHoles) hole.ClearSeeds();
        playerOneStorehouse.ClearSeeds();
        if (playerTwoStorehouse != null) playerTwoStorehouse.ClearSeeds();
    }

    // =================================================================
    // EVENT HANDLERS (Penanganan Event dari GameManager)
    // =================================================================

    // Fungsi 'inti' yang meng-unpause tutorial
    private IEnumerator HandleEventFlow(string message)
    {
        gameManager.SetGameActive(false); // Ambil alih kontrol
        gameManager.SetTutorialHole(null); // Matikan semua indikator
        yield return ShowMessage(message); // Tampilkan pesan, tunggu 'Next'
        isReadyForNextStep = true; // Sinyal ke 'StartTutorialSequence' untuk lanjut
    }

    // (HandleScoreUpdate DIHAPUS untuk perbaiki 'freeze')

    private void HandleSowContinue(Hole hole)
    {
        string msg = isIndonesian ?
            $"Bagus! Anda LANJUT menabur." :
            $"Good! You CONTINUE sowing.";
        StartCoroutine(HandleEventFlow(msg));
    }

    private void HandleCapture(Hole hole)
    {
        string msg = isIndonesian ?
            "MENEMBAK!\nAnda mengambil semua biji dari lubang seberang. Giliran Anda berakhir." :
            "CAPTURE!\nYou captured all seeds from the opposite hole. Your turn ends.";
        StartCoroutine(HandleEventFlow(msg));
    }

    private void HandleExtraTurn()
    {
        string msg = isIndonesian ?
            "GILIRAN TAMBAHAN!\nBiji terakhir mendarat di Lumbung Anda. Skor bertambah! Anda boleh main lagi." :
            "EXTRA TURN!\nThe last seed landed in your Storehouse. Your score increases! You get to play again.";
        
        // Di skenario Extra Turn, kita biarkan pemain main lagi
        gameManager.SetGameActive(false); // Ambil alih
        gameManager.SetTutorialHole(null); // Matikan semua indikator
        StartCoroutine(HandleEventFlow(msg));
    }

    private void HandleTurnEnd()
    {
        string msg = isIndonesian ?
            "Giliran Anda Berakhir.\nBiji terakhir mendarat di lubang kosong." :
            "Your Turn Ends.\nThe last seed landed in an empty hole.";
        StartCoroutine(HandleEventFlow(msg));
    }
}