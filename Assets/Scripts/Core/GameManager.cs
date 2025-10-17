using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Board References")]
    [SerializeField] private Hole[] playerOneHoles;
    [SerializeField] private Hole[] playerTwoHoles;
    [SerializeField] private Hole playerOneStorehouse;
    [SerializeField] private Hole playerTwoStorehouse;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < playerOneHoles.Length; i++) { playerOneHoles[i].SetIndex(i); playerOneHoles[i].SetHoleType(HoleType.PlayerOne); }
        for (int i = 0; i < playerTwoHoles.Length; i++) { playerTwoHoles[i].SetIndex(i); playerTwoHoles[i].SetHoleType(HoleType.PlayerTwo); }
        if (playerOneStorehouse != null) { playerOneStorehouse.SetHoleType(HoleType.Storehouse); playerOneStorehouse.SetIndex(-1); }
        if (playerTwoStorehouse != null) { playerTwoStorehouse.SetHoleType(HoleType.Storehouse); playerTwoStorehouse.SetIndex(-1); }
    }

    private void Start()
    {
        if (aiController != null)
        {
            aiController.Initialize(this, playerTwoHoles, playerOneHoles, playerTwoStorehouse);
        }
        RestartGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        pausePanel.SetActive(IsPaused);
        if (IsPaused && uiController != null)
        {
            uiController.UpdatePausePanelInfo(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
        }
    }

    public void RestartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic();
        StopAllCoroutines();
        SetupBoard();
    }

    private void SetupBoard()
    {
        foreach (var hole in playerOneHoles) hole.SetSeeds(startingSeeds);
        foreach (var hole in playerTwoHoles) hole.SetSeeds(startingSeeds);
        playerOneStorehouse.ClearSeeds();
        playerTwoStorehouse.ClearSeeds();
        currentPlayer = 1;
        isGameActive = true;
        isSowing = false;
        isAITurn = false; 
        if (uiController != null) uiController.HideGameOver();

        p1ExtraTurns = 0;
        p2ExtraTurns = 0;
        p1MaxSeedsCaptured = 0;
        p2MaxSeedsCaptured = 0;

        UpdateUIAndIndicators();
    }

    public void SelectHole(Hole hole)
    {
        if (!isGameActive || isSowing || IsPaused || !IsValidMove(hole)) return;

        isSowing = true;
        UpdateTurnIndicators();

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedPickup);
        StartCoroutine(SowSeedsCoroutine(hole));
    }

    private IEnumerator SowSeedsCoroutine(Hole startHole)
    {
        Hole currentHole = startHole;
        Hole lastHole = startHole;

        while (true)
        {
            List<GameObject> seedsToSow = currentHole.TakeAllSeedObjects();
            int seedCount = seedsToSow.Count;
            if (seedCount == 0) break;

            for (int i = 0; i < seedCount; i++)
            {
                Hole previousHole = lastHole;
                lastHole = GetNextHole(previousHole);

                if (sowDebugLogs)
                {
                    string prevName = previousHole != null ? previousHole.name : "null";
                    string nextName = lastHole != null ? lastHole.name : "null";
                    string nextType = lastHole != null ? lastHole.GetHoleType().ToString() : "null";
                    string nextIdx = (lastHole != null && lastHole.GetHoleType() != HoleType.Storehouse) ? lastHole.GetHoleIndex().ToString() : "store";
                    Debug.Log($"Sow step #{i}: {prevName} -> {nextName} (type:{nextType} idx:{nextIdx})");
                }

                if ((currentPlayer == 1 && lastHole == playerTwoStorehouse) ||
                    (currentPlayer == 2 && lastHole == playerOneStorehouse))
                {
                    if (sowDebugLogs) Debug.Log("Skipping opponent storehouse");
                    lastHole = GetNextHole(lastHole);
                }

                yield return StartCoroutine(lastHole.AnimateAddSeedCoroutine(
                    seedsToSow[i], previousHole.GetSeedContainer(), liftHeight, seedMoveSpeed));

                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedDrop);
                if (uiController != null) uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
            }

            bool isExtraTurn = (currentPlayer == 1 && lastHole == playerOneStorehouse) || (currentPlayer == 2 && lastHole == playerTwoStorehouse);

            if (isExtraTurn)
            {
                if (currentPlayer == 1) p1ExtraTurns++;
                else p2ExtraTurns++;
                
                if (uiController != null) uiController.UpdateGameStatus("Giliran Tambahan!");
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.extraTurn);
                isSowing = false;
                UpdateUIAndIndicators();
                CheckGameOver();

                if (!CheckGameOver() && config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2)
                {
                    isAITurn = true;
                    aiController.MakeMove(config.aiDifficulty);
                }
                yield break;
            }

            if (lastHole.GetSeedCount() == 1 && IsHoleBelongsToPlayer(lastHole, currentPlayer))
            {
                Hole opposite = GetOppositeHole(lastHole);
                if (opposite != null && opposite.GetSeedCount() > 0)
                {
                    int captured = opposite.ClearSeeds() + lastHole.ClearSeeds();
                    
                    if (currentPlayer == 1) {
                        if (captured > p1MaxSeedsCaptured) p1MaxSeedsCaptured = captured;
                    } else {
                        if (captured > p2MaxSeedsCaptured) p2MaxSeedsCaptured = captured;
                    }

                    Hole store = (currentPlayer == 1) ? playerOneStorehouse : playerTwoStorehouse;
                    store.AddSeeds(captured);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedCapture);
                }
                break;
            }

            if (lastHole.GetSeedCount() > 1 && lastHole.GetHoleType() != HoleType.Storehouse)
            {
                currentHole = lastHole;
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedPickup);
                yield return new WaitForSeconds(0.5f);
            }
            else { break; }
        }

        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        isSowing = false;
        isAITurn = false;
        UpdateUIAndIndicators();

        if (!CheckGameOver() && config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2)
        {
            isAITurn = true;
            aiController.MakeMove(config.aiDifficulty);
        }
    }
    
    public void EndTurnAfterAI()
    {
        currentPlayer = 1;
        isAITurn = false;
        UpdateUIAndIndicators();
        CheckGameOver();
    }

    private void UpdateUIAndIndicators()
    {
        if (uiController != null)
        {
            uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
        }
        UpdateTurnIndicators();
    }

    private void UpdateTurnIndicators()
    {
        if (isSowing || isAITurn)
        {
            foreach (var hole in playerOneHoles) hole.SetTurnHighlight(false);
            foreach (var hole in playerTwoHoles) hole.SetTurnHighlight(false);
            return;
        }

        foreach (var hole in playerOneHoles)
        {
            hole.SetTurnHighlight(currentPlayer == 1 && IsValidMove(hole));
        }
        foreach (var hole in playerTwoHoles)
        {
            hole.SetTurnHighlight(currentPlayer == 2 && IsValidMove(hole));
        }
    }

    private bool CheckGameOver()
    {
        if (!isGameActive) return true;
        bool p1SideEmpty = true; foreach (var hole in playerOneHoles) if (hole.GetSeedCount() > 0) p1SideEmpty = false;
        bool p2SideEmpty = true; foreach (var hole in playerTwoHoles) if (hole.GetSeedCount() > 0) p2SideEmpty = false;

        if (p1SideEmpty || p2SideEmpty)
        {
            isGameActive = false;
            if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();
            int p1Remaining = 0; foreach (var hole in playerOneHoles) p1Remaining += hole.ClearSeeds(); playerOneStorehouse.AddSeeds(p1Remaining);
            int p2Remaining = 0; foreach (var hole in playerTwoHoles) p2Remaining += hole.ClearSeeds(); playerTwoStorehouse.AddSeeds(p2Remaining);
            UpdateUIAndIndicators();
            int p1Score = playerOneStorehouse.GetSeedCount();
            int p2Score = playerTwoStorehouse.GetSeedCount();
            AudioClip finalSfx; bool isWin = p1Score > p2Score; bool isDraw = p1Score == p2Score;
            if (isDraw) { finalSfx = AudioManager.Instance.gameWin; } else if (isWin) { finalSfx = AudioManager.Instance.gameWin; } else { finalSfx = (config != null && config.gameMode == GameMode.PlayerVsAI) ? AudioManager.Instance.gameLose : AudioManager.Instance.gameWin; }
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(finalSfx);
            if (uiController != null)
            {
                uiController.ShowGameOver(isWin, isDraw);
                uiController.UpdateGameEndStats(p1Score, p2Score, p1MaxSeedsCaptured, p2MaxSeedsCaptured, p1ExtraTurns, p2ExtraTurns);
            }
            return true;
        }
        return false;
    }

    public bool IsValidMove(Hole hole) { if (hole == null || hole.GetHoleType() == HoleType.Storehouse || hole.GetSeedCount() == 0) return false; if (currentPlayer == 1 && hole.GetHoleType() != HoleType.PlayerOne) return false; if (currentPlayer == 2 && hole.GetHoleType() != HoleType.PlayerTwo) return false; return true; }
    private bool IsHoleBelongsToPlayer(Hole hole, int player) { return (player == 1 && hole.GetHoleType() == HoleType.PlayerOne) || (player == 2 && hole.GetHoleType() == HoleType.PlayerTwo); }
    private Hole GetOppositeHole(Hole hole) { if (hole.GetHoleType() == HoleType.PlayerOne) { int idx = hole.GetHoleIndex(); return playerTwoHoles[playerTwoHoles.Length - 1 - idx]; } else if (hole.GetHoleType() == HoleType.PlayerTwo) { int idx = hole.GetHoleIndex(); return playerOneHoles[playerOneHoles.Length - 1 - idx]; } return null; }

    public Hole GetNextHolePublic(Hole current)
    {
        return GetNextHole(current);
    }
    
    private Hole GetNextHole(Hole current)
    {
        if (current.GetHoleType() == HoleType.PlayerOne)
        {
            int idx = current.GetHoleIndex();
            if (idx < playerOneHoles.Length - 1)
                return playerOneHoles[idx + 1];
            else
                return playerOneStorehouse;
        }
        else if (current == playerOneStorehouse)
        {
            return playerTwoHoles[0];
        }
        else if (current.GetHoleType() == HoleType.PlayerTwo)
        {
            int idx = current.GetHoleIndex();
            if (idx < playerTwoHoles.Length - 1)
                return playerTwoHoles[idx + 1];
            else
                return playerTwoStorehouse;
        }
        else if (current == playerTwoStorehouse)
        {
            return playerOneHoles[0];
        }
        return null;
    }
}