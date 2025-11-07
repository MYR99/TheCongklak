using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Board References")]
    [SerializeField] private Hole[] playerOneHoles; // P1_0 (kanan) -> P1_6 (kiri)
    [SerializeField] private Hole[] playerTwoHoles; // P2_0 (kanan) -> P2_6 (kiri)
    [SerializeField] private Hole playerOneStorehouse; // Kiri
    [SerializeField] public Hole playerTwoStorehouse; // Kanan (dibuat public)

    [Header("Gameplay Settings")]
    [SerializeField] private int startingSeeds = 7;
    [SerializeField] private float seedMoveSpeed = 5f;
    [SerializeField] private float liftHeight = 0.5f;

    [Header("References")]
    [SerializeField] private UIController uiController;
    [SerializeField] public GameConfig config; 
    [SerializeField] public GameObject pausePanel;
    [SerializeField] private bool sowDebugLogs = false;
    [SerializeField] private AIController aiController;

    [Header("Tutorial")]
    [SerializeField] private TutorialController tutorialController;
    private bool isTutorialActive = false; 
    private Hole currentTutorialHole = null; // Lubang yang boleh diklik

    // --- Tutorial Events ---
    public static event Action<Hole> OnTutorialSowContinue;
    public static event Action<Hole> OnTutorialCapture;
    public static event Action OnTutorialExtraTurn;
    public static event Action OnTutorialTurnEnd;
    public static event Action<int, int> OnTutorialScoreUpdate;

    // --- Game State ---
    private int currentPlayer = 1;
    private bool isSowing = false;
    private bool isGameActive = true;
    private bool isAITurn = false; 
    private int p1ExtraTurns;
    private int p2ExtraTurns;
    private int p1MaxSeedsCaptured;
    private int p2MaxSeedsCaptured;

    public bool IsPaused { get; private set; } = false;
    public int CurrentPlayer => currentPlayer;
    public bool IsGameActive => isGameActive;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null) { Instance = this; } 
        else { Destroy(gameObject); }

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("GameConfig belum di-assign di GameManager!");
            return;
        }

        startingSeeds = config.initialSeedsPerHole;
        
        if (config.gameMode == GameMode.PlayerVsAI && !config.tutorialCompleted && tutorialController != null)
        {
            StartCoroutine(TutorialSequence());
        }
        else
        {
            isTutorialActive = false;
            SetupBoard();
            StartGame();
        }
    }

    #endregion

    #region Game Flow

    private IEnumerator TutorialSequence()
    {
        isTutorialActive = true;
        isGameActive = false; // Blok input sampai tutorial menyuruh
        isAITurn = true; // Blok AI
        
        SetupBoard(); // <-- MENGGUNAKAN LOGIKA ASLI (7 BIJI PER LUBANG)
        yield return tutorialController.StartTutorialSequence(); 
    }

    private void StartGame()
    {
        isGameActive = true;
        currentPlayer = 1;
        UpdateTurnIndicators(); // Panggil fungsi yang sudah di-update
        
        if (config.gameMode == GameMode.PlayerVsAI)
        {
            if (aiController == null) aiController = FindObjectOfType<AIController>();
            aiController.Initialize(this, playerTwoHoles, playerOneHoles, playerTwoStorehouse, playerOneStorehouse);
            isAITurn = false;
        }
        
        if (uiController != null)
        {
            uiController.UpdateScores(0, 0, currentPlayer);
            uiController.UpdateGameStatus("Player 1's Turn");
        }
    }

    public void RestartGame()
    {
        isSowing = false;
        isGameActive = true;
        isAITurn = false;
        isTutorialActive = false; 
        currentPlayer = 1;
        
        p1ExtraTurns = 0;
        p2ExtraTurns = 0;
        p1MaxSeedsCaptured = 0;
        p2MaxSeedsCaptured = 0;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            IsPaused = false;
        }
        
        if (uiController != null)
        {
            uiController.HideGameOver();
        }

        SetupBoard();
        StartGame();
    }
    
    public void EndTutorial()
    {
        isTutorialActive = false;
        config.tutorialCompleted = true; // Tandai selesai
        currentTutorialHole = null; // Hapus paksaan lubang
        RestartGame(); // Mulai ulang game
    }

    public void SetupBoard()
    {
        // Menggunakan logika asli dari config (7 biji)
        foreach (var hole in playerOneHoles) hole.SetSeeds(startingSeeds);
        foreach (var hole in playerTwoHoles) hole.SetSeeds(startingSeeds);
        playerOneStorehouse.ClearSeeds();
        playerTwoStorehouse.ClearSeeds();
    }

    private void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnIndicators();

        if (uiController != null) uiController.UpdateGameStatus($"Player {currentPlayer}'s Turn");

        if (config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2)
        {
            isAITurn = true;
            StartCoroutine(HandleAITurn());
        }
        else
        {
            isAITurn = false;
        }
    }

    private IEnumerator HandleAITurn()
    {
        isGameActive = false; 
        yield return new WaitForSeconds(1.0f); 
        
        if (aiController != null)
        {
            aiController.MakeMove(config.aiDifficulty);
        }
        else
        {
            Debug.LogError("AIController is not assigned!");
        }
    }

    private void UpdateTurnIndicators()
    {
        // --- LOGIKA BARU UNTUK INDIKATOR TUTORIAL ---
        if (isTutorialActive)
        {
            // Matikan semua indikator
            foreach (var hole in playerOneHoles) hole.turnIndicatorHighlight.SetActive(false);
            foreach (var hole in playerTwoHoles) hole.turnIndicatorHighlight.SetActive(false);
            
            // Nyalakan HANYA lubang yang disuruh
            if (currentTutorialHole != null)
            {
                currentTutorialHole.turnIndicatorHighlight.SetActive(true);
            }
            return; // Hentikan di sini
        }
        // ---------------------------------------------
        
        // Logika normal (non-tutorial)
        foreach (var hole in playerOneHoles) hole.turnIndicatorHighlight.SetActive(currentPlayer == 1);
        foreach (var hole in playerTwoHoles) hole.turnIndicatorHighlight.SetActive(currentPlayer == 2);
    }
    
    private void CheckGameOver()
    {
        bool p1SideEmpty = true;
        foreach (var hole in playerOneHoles)
        {
            if (hole.GetSeedCount() > 0)
            {
                p1SideEmpty = false;
                break;
            }
        }

        bool p2SideEmpty = true;
        foreach (var hole in playerTwoHoles)
        {
            if (hole.GetSeedCount() > 0)
            {
                p2SideEmpty = false;
                break;
            }
        }

        if (p1SideEmpty || p2SideEmpty)
        {
            isGameActive = false;
            if (sowDebugLogs) Debug.Log("[GAME OVER] One side is empty.");
            
            // Perbaikan error kompilasi
            int p1FinalScore = playerOneStorehouse.GetSeedCount();
            int p2FinalScore = playerTwoStorehouse.GetSeedCount();
            
            if (p1SideEmpty)
            {
                int p2Sweep = 0;
                foreach (var hole in playerTwoHoles) p2Sweep += hole.ClearSeeds();
                p2FinalScore += p2Sweep; 
            }
            
            if (p2SideEmpty)
            {
                int p1Sweep = 0;
                foreach (var hole in playerOneHoles) p1Sweep += hole.ClearSeeds();
                p1FinalScore += p1Sweep; 
            }

            if (uiController != null)
            {
                // Sesuai dengan UIController.cs Anda (ShowGameOver(bool, bool))
                bool isDraw = p1FinalScore == p2FinalScore;
                bool isWin = false; 

                if (config.gameMode == GameMode.PlayerVsAI)
                {
                    isWin = p1FinalScore > p2FinalScore;
                }
                else 
                {
                    isWin = p1FinalScore > p2FinalScore;
                }
                
                uiController.ShowGameOver(isWin, isDraw);
            }
        }
    }
    
    #endregion

    #region Sowing Logic

    public void StartSowing(Hole startHole)
    {
        if (isSowing) return;

        isSowing = true;
        isGameActive = false;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedPickup);
        StartCoroutine(SowSeedsCoroutine(startHole));
    }

    private IEnumerator SowSeedsCoroutine(Hole startHole)
    {
        Hole currentHole = startHole;
        Hole lastHole = startHole;

        while (currentHole != null)  // Changed from while(true)
        {
            // Prevent sowing from storehouse (lumbung)
            if (currentHole.GetHoleType() == HoleType.Storehouse)
            {
                if (sowDebugLogs) Debug.Log("[SOW] Prevent sowing from storehouse, breaking loop.");
                break;
            }

            List<GameObject> seedsToSow = currentHole.TakeAllSeedObjects();
            int seedCount = seedsToSow.Count;

            if (seedCount == 0)
            {
                if (sowDebugLogs) Debug.Log("[SOW] No seeds to sow, breaking loop.");
                break;
            }

            if (isTutorialActive && currentHole != startHole)
            {
                OnTutorialSowContinue?.Invoke(currentHole);
            }

            for (int i = 0; i < seedCount; i++)
            {
                Hole previousHole = lastHole;
                lastHole = GetNextHole(previousHole);

                if (lastHole == null)
                {
                    Debug.LogError("[SOW] Got null for next hole, breaking sowing sequence");
                    break;
                }

                if (sowDebugLogs)
                    Debug.Log($"[SOW] P{currentPlayer} sowing. Moving seed {i + 1}/{seedCount} from {previousHole.name} to {lastHole.name}");

                // Always skip opponent's storehouse
                if ((currentPlayer == 1 && lastHole == playerTwoStorehouse) ||
                    (currentPlayer == 2 && lastHole == playerOneStorehouse))
                {
                    if (sowDebugLogs) Debug.Log($"[SOW] P{currentPlayer} skipping opponent storehouse {lastHole.name}");
                    lastHole = GetNextHole(lastHole);
                }

                if (lastHole == null)
                {
                    Debug.LogError("[SOW] Got null after skipping storehouse, breaking sowing sequence");
                    break;
                }

                float currentMoveSpeed = seedMoveSpeed;
                if (config != null && config.isFastForwardActive)
                    currentMoveSpeed *= config.fastForwardSpeedMultiplier;

                yield return StartCoroutine(lastHole.AnimateAddSeedCoroutine(
                    seedsToSow[i], previousHole.GetSeedContainer(), liftHeight, currentMoveSpeed));

                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedDrop);

                if (isTutorialActive)
                {
                    if (lastHole == playerOneStorehouse)
                        OnTutorialScoreUpdate?.Invoke(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount());
                }

                if (uiController != null) uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
            }

            if (lastHole == null) break;

            // Determine if we should continue sowing
            if (lastHole.GetHoleType() == HoleType.Storehouse)
            {
                if (sowDebugLogs) Debug.Log($"[SOW] Landed in storehouse {lastHole.name}, ending sowing.");
                currentHole = null; // End turn or extra turn
            }
            else if (lastHole.GetSeedCount() == 1)
            {
                if (sowDebugLogs) Debug.Log($"[SOW] Landed in empty hole {lastHole.name}, ending sowing.");
                currentHole = null; // End turn or capture
            }
            else // Landed in a hole with seeds
            {
                if (sowDebugLogs) Debug.Log($"[SOW] Landed in non-empty hole {lastHole.name} ({lastHole.GetSeedCount()} seeds), continuing sowing.");
                currentHole = lastHole; // Continue sowing from this hole
            }
        }

        if (lastHole != null)
        {
            EndTurnSequence(lastHole);
        }

        isSowing = false;

        if (!isTutorialActive)
        {
            CheckGameOver();
            if (!isAITurn) isGameActive = true;
        }
    }
    
    private void EndTurnSequence(Hole lastHole)
    {
        // --- PERBAIKAN LOGIKA 'NEMBAK' ---
        bool wasCapture = false;
        // Cek 'nembak' HANYA jika mendarat di lubang (bukan lumbung) dan bijinya cuma 1
        if (lastHole.GetHoleType() != HoleType.Storehouse && lastHole.GetSeedCount() == 1)
        {
            Hole oppositeHole = GetOppositeHole(lastHole);
            if (oppositeHole != null && oppositeHole.GetSeedCount() > 0) // Cek lubang seberang ADA ISI
            {
                // Cek apakah lubang ini milik pemain saat ini
                wasCapture = (lastHole.GetHoleType() == HoleType.PlayerOne && currentPlayer == 1) ||
                             (lastHole.GetHoleType() == HoleType.PlayerTwo && currentPlayer == 2);
            }
        }
        // ----------------------------------
        
        bool isExtraTurn = (currentPlayer == 1 && lastHole == playerOneStorehouse) ||
                           (currentPlayer == 2 && lastHole == playerTwoStorehouse);

        if (isTutorialActive)
        {
            if (isExtraTurn)
            {
                OnTutorialExtraTurn?.Invoke();
            }
            else if (wasCapture)
            {
                HandleCapture(lastHole); 
                OnTutorialCapture?.Invoke(lastHole); 
            }
            else
            {
                OnTutorialTurnEnd?.Invoke();
            }
        }
        else
        {
            if (isExtraTurn)
            {
                if (sowDebugLogs) Debug.Log($"[SOW] P{currentPlayer} landed in storehouse. Extra turn!");
                if (uiController != null) uiController.UpdateGameStatus($"Player {currentPlayer} gets an extra turn!");
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.extraTurn);
                
                p1ExtraTurns += (currentPlayer == 1) ? 1 : 0;
                p2ExtraTurns += (currentPlayer == 2) ? 1 : 0;
                
                if (config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2)
                {
                    isAITurn = true;
                    StartCoroutine(HandleAITurn());
                }
            }
            else if (wasCapture)
            {
                HandleCapture(lastHole);
                SwitchPlayer(); 
            }
            else
            {
                SwitchPlayer();
            }
        }
    }

    private void HandleCapture(Hole lastHole)
    {
        Hole oppositeHole = GetOppositeHole(lastHole);
        if (oppositeHole == null || oppositeHole.GetSeedCount() == 0) return;

        if (sowDebugLogs) Debug.Log($"[SOW] P{currentPlayer} CAPTURED hole {oppositeHole.name}");

        int capturedSeeds = oppositeHole.ClearSeeds();
        int capturingSeed = lastHole.ClearSeeds();
        
        int totalCaptured = capturedSeeds + capturingSeed;
        if (currentPlayer == 1) p1MaxSeedsCaptured = Mathf.Max(p1MaxSeedsCaptured, totalCaptured);
        else p2MaxSeedsCaptured = Mathf.Max(p2MaxSeedsCaptured, totalCaptured);

        Hole targetStorehouse = (currentPlayer == 1) ? playerOneStorehouse : playerTwoStorehouse;

        for (int i = 0; i < totalCaptured; i++)
        {
            GameObject seed = SeedPool.Instance.GetSeed();
            StartCoroutine(targetStorehouse.AnimateAddSeedCoroutine(seed, targetStorehouse.GetSeedContainer(), liftHeight, seedMoveSpeed * 2));
        }
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedCapture);
        if (uiController != null) uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
    }
    
    #endregion

    #region Helper Functions
    
    // Dipanggil oleh PlayerInputController
    public void SelectHole(Hole hole)
    {
        if (hole == null) return;
        if (!IsValidMove(hole))
        {
            if (sowDebugLogs) Debug.Log($"SelectHole: invalid move attempted on {hole.name}");
            return;
        }
        StartSowing(hole);
    }

    // Dipanggil oleh HoleHighlight
    public bool IsValidMove(Hole hole)
    {
        if (hole == null || !isGameActive || isSowing || IsPaused || hole.GetSeedCount() == 0)
            return false;

        if (currentPlayer == 1 && hole.GetHoleType() != HoleType.PlayerOne) return false;
        if (currentPlayer == 2 && hole.GetHoleType() != HoleType.PlayerTwo) return false;
        if (config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2) return false;

        // --- LOGIKA BARU UNTUK TUTORIAL ---
        if (isTutorialActive)
        {
            if (currentTutorialHole != null && hole != currentTutorialHole)
            {
                return false; // Jika tutorial aktif & menunggu lubang, hanya izinkan lubang itu
            }
            if (currentTutorialHole == null && !isSowing)
            {
                return false; // Jika tutorial aktif & tidak menunggu lubang (misal: panel teks muncul), blok semua
            }
        }
        // ----------------------------------

        return true;
    }

    public Hole GetOppositeHole(Hole hole)
    {
        try
        {
            if (hole.GetHoleType() == HoleType.PlayerOne)
            {
                // P1_0 (idx 0) -> P2_0 (idx 0) (Logika lurus)
                return playerTwoHoles[hole.GetHoleIndex()]; 
            }
            else if (hole.GetHoleType() == HoleType.PlayerTwo)
            {
                return playerOneHoles[hole.GetHoleIndex()];
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GetOppositeHole] Error: {e.Message}. Hole: {hole.name}, Index: {hole.GetHoleIndex()}");
        }
        return null;
    }

    public Hole GetNextHolePublic(Hole current)
    {
        return GetNextHole(current);
    }

    private Hole GetNextHole(Hole current)
    {
        try
        {
            if (current.GetHoleType() == HoleType.PlayerOne)
            {
                int idx = current.GetHoleIndex();
                if (idx < playerOneHoles.Length - 1) // index 0-5
                {
                    return playerOneHoles[idx + 1]; // return 1-6
                }
                else // index 6
                {
                    return playerOneStorehouse; // return lumbung
                }
            }
            else if (current == playerOneStorehouse)
            {
                return playerTwoHoles[0]; // Dari lumbung P1, lanjut ke P2
            }
            else if (current.GetHoleType() == HoleType.PlayerTwo)
            {
                int idx = current.GetHoleIndex();
                if (idx < playerTwoHoles.Length - 1) // index 0-5
                {
                    return playerTwoHoles[idx + 1]; // return 1-6
                }
                else // index 6
                {
                    return playerTwoStorehouse; // return lumbung
                }
            }
            else if (current == playerTwoStorehouse)
            {
                return playerOneHoles[0]; // Dari lumbung P2, kembali ke P1
            }
            
            Debug.LogError($"[GetNextHole] Unknown hole type or state: {current.name}");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GetNextHole] Error getting next hole: {e.Message} (Hole: {current.name})");
            return null;
        }
    }

    public void EndTurnAfterAI()
    {
        isAITurn = false;
        isGameActive = true;
        SwitchPlayer();
    }

    public void SetFastForward(bool isOn)
    {
        if (config == null) return;
        config.isFastForwardActive = isOn;
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        pausePanel.SetActive(IsPaused);
        
        if (IsPaused)
        {
            if (uiController != null) uiController.UpdatePausePanelInfo(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void SetGameActive(bool isActive)
    {
        isGameActive = isActive;
    }
    
    // --- FUNGSI BARU UNTUK TUTORIAL ---
    public void SetTutorialHole(Hole hole)
    {
        currentTutorialHole = hole;
        UpdateTurnIndicators(); // Perbarui lampu indikator
    }
    
    // Enum ini tidak lagi dipakai di sini
    // public enum TutorialScenario 
    // { ... }
    // ----------------------------------

    #endregion
}