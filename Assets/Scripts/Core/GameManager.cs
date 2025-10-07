using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIController uiController;
    [SerializeField] private GameConfig config; 

    [SerializeField] public GameObject pausePanel;
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; 
            pausePanel.SetActive(true);
            
            if (uiController != null)
            {
                uiController.UpdatePausePanelInfo(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
            }
        }
        else
        {
            Time.timeScale = 1f; 
            pausePanel.SetActive(false);
        }
    }
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

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private int currentPlayer = 1;
    private bool isSowing = false;
    private bool isGameActive = true;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < playerOneHoles.Length; i++) { playerOneHoles[i].SetIndex(i); playerOneHoles[i].SetHoleType(HoleType.PlayerOne); }
        for (int i = 0; i < playerTwoHoles.Length; i++) { playerTwoHoles[i].SetIndex(i); playerTwoHoles[i].SetHoleType(HoleType.PlayerTwo); }
        playerOneStorehouse.SetHoleType(HoleType.Storehouse);
        playerTwoStorehouse.SetHoleType(HoleType.Storehouse);
    }

    private void Start()
    {
        if (AudioManager.Instance != null) { AudioManager.Instance.StartMusic(); }
        RestartGame();
    }
    
    public void RestartGame()
    {
        StopAllCoroutines();
        isSowing = false;
        SetupBoard();
        if (showDebug) Debug.Log("[GameManager] Game restarted");
    }

    private void SetupBoard()
    {
        foreach (var hole in playerOneHoles) hole.SetSeeds(startingSeeds);
        foreach (var hole in playerTwoHoles) hole.SetSeeds(startingSeeds);
        playerOneStorehouse.ClearSeeds();
        playerTwoStorehouse.ClearSeeds();
        currentPlayer = 1;
        isGameActive = true;
        if (uiController != null) uiController.HideGameOver();
        UpdateUI();
        if (showDebug) Debug.Log("[GameManager] Board setup complete");
        UpdateTurnIndicators();
    }
    
    public void SelectHole(Hole hole)
    {
        if (!isGameActive || isSowing || !IsValidMove(hole)) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedPickup);
        StartCoroutine(SowSeedsCoroutine(hole));
    }
    
    private IEnumerator SowSeedsCoroutine(Hole startHole)
    {
        isSowing = true;
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
                
                yield return StartCoroutine(lastHole.AnimateAddSeedCoroutine(
                    seedsToSow[i], previousHole.GetSeedContainer(), liftHeight, seedMoveSpeed));

                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedDrop);
                UpdateUI();
            }

            bool isExtraTurn = (currentPlayer == 1 && lastHole == playerOneStorehouse) || (currentPlayer == 2 && lastHole == playerTwoStorehouse);

            if (isExtraTurn)
            {
                uiController.UpdateGameStatus("Giliran Tambahan!");
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.extraTurn);
                CheckGameOver();
                isSowing = false; 
                yield break; 
            }
            
            if (lastHole.GetSeedCount() == 1 && IsHoleBelongsToPlayer(lastHole, currentPlayer))
            {
                Hole opposite = GetOppositeHole(lastHole);
                if (opposite != null && opposite.GetSeedCount() > 0)
                {
                    if (showDebug) Debug.Log($"[GameManager] Player {currentPlayer} MENEMBAK lubang {opposite.name}");
                    int captured = opposite.ClearSeeds() + lastHole.ClearSeeds();
                    Hole store = (currentPlayer == 1) ? playerOneStorehouse : playerTwoStorehouse;
                    store.AddSeeds(captured);
                    
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedCapture);
                    UpdateUI();
                }
                break;
            }

            if (lastHole.GetSeedCount() > 1)
            {
                if (showDebug) Debug.Log("[GameManager] Melanjutkan menabur dari " + lastHole.name);
                currentHole = lastHole;
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedPickup);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                break;
            }
        }

        CheckGameOver();
        if(isGameActive)
        {
            bool isExtraTurn = (currentPlayer == 1 && lastHole == playerOneStorehouse) || (currentPlayer == 2 && lastHole == playerTwoStorehouse);
            if(!isExtraTurn)
            {
                currentPlayer = (currentPlayer == 1) ? 2 : 1;
            }
            UpdateUI();
            UpdateTurnIndicators();
        }
        isSowing = false;
    }

    private void CheckGameOver()
    {
        bool p1SideEmpty = true;
        foreach(var hole in playerOneHoles) if(hole.GetSeedCount() > 0) p1SideEmpty = false;

        bool p2SideEmpty = true;
        foreach(var hole in playerTwoHoles) if(hole.GetSeedCount() > 0) p2SideEmpty = false;

        if (p1SideEmpty || p2SideEmpty)
        {
            isGameActive = false;
            int p1Remaining = 0; foreach (var hole in playerOneHoles) p1Remaining += hole.ClearSeeds(); playerOneStorehouse.AddSeeds(p1Remaining);
            int p2Remaining = 0; foreach (var hole in playerTwoHoles) p2Remaining += hole.ClearSeeds(); playerTwoStorehouse.AddSeeds(p2Remaining);
            UpdateUI();

            int p1Score = playerOneStorehouse.GetSeedCount();
            int p2Score = playerTwoStorehouse.GetSeedCount();

            AudioClip finalSfx = null;

            if (p1Score > p2Score)
            {
                finalSfx = AudioManager.Instance.gameWin;
                uiController.ShowGameOver(true); 
            }
            else if (p2Score > p1Score)
            {
                finalSfx = (config != null && config.gameMode == GameMode.PlayerVsAI) ? AudioManager.Instance.gameLose : AudioManager.Instance.gameWin;
                uiController.ShowGameOver(false); 
            }
            else
            {
                finalSfx = AudioManager.Instance.gameWin;
                uiController.ShowGameOver(true, true); 
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(finalSfx);
        }
    }

    private void UpdateUI()
    {
        if (uiController != null)
        {
            uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
        }
    }
    private void UpdateTurnIndicators()
    {
        foreach(var hole in playerOneHoles)
        {
            hole.SetTurnHighlight(currentPlayer == 1 && IsValidMove(hole));
        }
        foreach(var hole in playerTwoHoles)
        {
            hole.SetTurnHighlight(currentPlayer == 2 && IsValidMove(hole));
        }
    }
    
    public bool IsValidMove(Hole hole) { if (hole == null || hole.GetHoleType() == HoleType.Storehouse || hole.GetSeedCount() == 0) return false; if (currentPlayer == 1 && hole.GetHoleType() != HoleType.PlayerOne) return false; if (currentPlayer == 2 && hole.GetHoleType() != HoleType.PlayerTwo) return false; return true; }
    private bool IsHoleBelongsToPlayer(Hole hole, int player) { return (player == 1 && hole.GetHoleType() == HoleType.PlayerOne) || (player == 2 && hole.GetHoleType() == HoleType.PlayerTwo); }
    private Hole GetOppositeHole(Hole hole) { if (hole.GetHoleType() == HoleType.PlayerOne) { int idx = hole.GetHoleIndex(); return playerTwoHoles[idx]; } else if (hole.GetHoleType() == HoleType.PlayerTwo) { int idx = hole.GetHoleIndex(); return playerOneHoles[idx]; } return null; }
    private Hole GetNextHole(Hole current) { if (currentPlayer == 1) { if (current.GetHoleType() == HoleType.PlayerOne) { int idx = current.GetHoleIndex(); if (idx > 0) return playerOneHoles[idx - 1]; else return playerOneStorehouse; } else if (current == playerOneStorehouse) { return playerTwoHoles[0]; } else { int idx = current.GetHoleIndex(); if (idx < playerTwoHoles.Length - 1) return playerTwoHoles[idx + 1]; else return playerOneHoles[playerOneHoles.Length - 1]; } } else { if (current.GetHoleType() == HoleType.PlayerTwo) { int idx = current.GetHoleIndex(); if (idx < playerTwoHoles.Length - 1) return playerTwoHoles[idx + 1]; else return playerTwoStorehouse; } else if (current == playerTwoStorehouse) { return playerOneHoles[playerOneHoles.Length - 1]; } else { int idx = current.GetHoleIndex(); if (idx > 0) return playerOneHoles[idx - 1]; else return playerTwoHoles[0]; } } return null; }
}