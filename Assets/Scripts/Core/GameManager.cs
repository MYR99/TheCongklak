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
    [SerializeField] private float seedMoveSpeed = 2f;
    [SerializeField] private float liftHeight = 0.8f;
    [SerializeField] private float moveDuration = 0.3f;

    [Header("References")]
    [SerializeField] private UIController uiController;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private int currentPlayer = 1;
    private bool isSowing = false;
    private bool isGameActive = true;

    public int CurrentPlayer => currentPlayer;
    public bool IsGameActive => isGameActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Auto set type & index
        for (int i = 0; i < playerOneHoles.Length; i++)
        {
            playerOneHoles[i].SetIndex(i);
            playerOneHoles[i].SetHoleType(HoleType.PlayerOne);
        }

        for (int i = 0; i < playerTwoHoles.Length; i++)
        {
            playerTwoHoles[i].SetIndex(i);
            playerTwoHoles[i].SetHoleType(HoleType.PlayerTwo);
        }

        playerOneStorehouse.SetHoleType(HoleType.Storehouse);
        playerTwoStorehouse.SetHoleType(HoleType.Storehouse);
    }

    private void Start()
    {
        SetupBoard();
    }

    // ========= SETUP =========
    private void SetupBoard()
    {
        foreach (var hole in playerOneHoles) hole.SetSeeds(startingSeeds);
        foreach (var hole in playerTwoHoles) hole.SetSeeds(startingSeeds);

        playerOneStorehouse.ClearSeeds();
        playerTwoStorehouse.ClearSeeds();

        currentPlayer = 1;
        isGameActive = true;

        UpdateUI();
        if (showDebug) Debug.Log("[GameManager] Board setup complete");
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        SetupBoard();
        if (showDebug) Debug.Log("[GameManager] Game restarted");
    }

    // ========= INPUT =========
    public void SelectHole(Hole hole)
    {
        if (!isGameActive) return;
        if (IsValidMove(hole))
        {
            PlayMove(hole);
        }
        else
        {
            if (showDebug) Debug.Log("[GameManager] Invalid hole selected: " + hole.name);
        }
    }

    public bool IsValidMove(Hole hole)
    {
        if (hole == null) return false;
        if (hole.GetHoleType() == HoleType.Storehouse) return false;
        if (hole.GetSeedCount() == 0) return false;

        if (currentPlayer == 1 && hole.GetHoleType() != HoleType.PlayerOne) return false;
        if (currentPlayer == 2 && hole.GetHoleType() != HoleType.PlayerTwo) return false;

        return true;
    }

    public void PlayMove(Hole hole)
    {
        if (isSowing || !IsValidMove(hole)) return;
        StartCoroutine(SowSeedsCoroutine(hole));
    }

    // ========= SOWING =========
    private IEnumerator SowSeedsCoroutine(Hole startHole)
    {
        isSowing = true;
        List<GameObject> seedsToSow = startHole.TakeAllSeedObjects();
        int seedCount = seedsToSow.Count;
        Hole currentHole = startHole;

        if (showDebug) Debug.Log($"[Sow] Player {currentPlayer} sowing {seedCount} seeds from {startHole.name}");

        for (int i = 0; i < seedCount; i++)
        {
            currentHole = GetNextHole(currentHole);
            GameObject seed = seedsToSow[i];
            yield return AnimateSeed(seed, currentHole);
            if (showDebug) Debug.Log($"[Sow] Seed {i + 1}/{seedCount} landed in {currentHole.name}");
        }

        yield return HandleTurnEnd(currentHole);

        UpdateUI();
        isSowing = false;
    }

    private IEnumerator AnimateSeed(GameObject seed, Hole targetHole)
    {
        if (seed == null || targetHole == null) yield break;

        Vector3 start = seed.transform.position;
        Vector3 lift = start + Vector3.up * liftHeight;
        Vector3 dropTarget = targetHole.transform.position + targetHole.GetRandomSeedPosition();

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime * seedMoveSpeed;
            seed.transform.position = Vector3.Lerp(start, lift, elapsed / moveDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime * seedMoveSpeed;
            seed.transform.position = Vector3.Lerp(lift, dropTarget, elapsed / moveDuration);
            yield return null;
        }

        targetHole.AddSeedObject(seed);
    }

    // ========= TURN RULES =========
    private IEnumerator HandleTurnEnd(Hole lastHole)
    {
        if (lastHole == null) yield break;

        if (showDebug) Debug.Log($"[Turn] Player {currentPlayer} last seed landed in {lastHole.name}");

        // Extra turn
        if ((currentPlayer == 1 && lastHole == playerOneStorehouse) ||
            (currentPlayer == 2 && lastHole == playerTwoStorehouse))
        {
            if (showDebug) Debug.Log("[Turn] Extra turn for Player " + currentPlayer);
            uiController.UpdateGameStatus($"Player {currentPlayer} dapat giliran tambahan!");
            yield break;
        }

        // Capture
        if (lastHole.GetSeedCount() == 1 &&
            lastHole.GetHoleType() != HoleType.Storehouse &&
            IsHoleBelongsToPlayer(lastHole, currentPlayer))
        {
            Hole opposite = GetOppositeHole(lastHole);
            if (opposite != null && opposite.GetSeedCount() > 0)
            {
                int captured = opposite.ClearSeeds();
                int self = lastHole.ClearSeeds();
                Hole store = (currentPlayer == 1) ? playerOneStorehouse : playerTwoStorehouse;
                store.AddSeeds(captured + self);

                if (showDebug) Debug.Log($"[Capture] Player {currentPlayer} captured {captured + self} seeds");
            }
        }

        // Relay sowing
        if (lastHole.GetSeedCount() > 1 && lastHole.GetHoleType() != HoleType.Storehouse)
        {
            if (showDebug) Debug.Log("[Relay] Continue sowing");
            yield return SowSeedsCoroutine(lastHole);
            yield break;
        }

        // Switch turn
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        uiController.UpdateTurnUI(currentPlayer);
        if (showDebug) Debug.Log($"[Turn] Switching turn to Player {currentPlayer}");
    }

    private bool IsHoleBelongsToPlayer(Hole hole, int player)
    {
        return (player == 1 && hole.GetHoleType() == HoleType.PlayerOne) ||
               (player == 2 && hole.GetHoleType() == HoleType.PlayerTwo);
    }

    private Hole GetNextHole(Hole current)
    {
        if (currentPlayer == 1)
        {
            if (current.GetHoleType() == HoleType.PlayerOne)
            {
                int idx = current.GetHoleIndex();
                if (idx > 0) return playerOneHoles[idx - 1];
                else return playerOneStorehouse; // setelah index 0 baru ke storehouse
            }
            else if (current.GetHoleType() == HoleType.Storehouse)
            {
                return playerTwoHoles[0]; // masuk ke sisi lawan dari kiri
            }
            else if (current.GetHoleType() == HoleType.PlayerTwo)
            {
                int idx = current.GetHoleIndex();
                if (idx < playerTwoHoles.Length - 1) return playerTwoHoles[idx + 1];
                else return playerOneHoles[playerOneHoles.Length - 1]; // kembali ke kanan PlayerOne
            }
        }
        else // PlayerTwo
        {
            if (current.GetHoleType() == HoleType.PlayerTwo)
            {
                int idx = current.GetHoleIndex();
                if (idx < playerTwoHoles.Length - 1) return playerTwoHoles[idx + 1];
                else return playerTwoStorehouse;
            }
            else if (current.GetHoleType() == HoleType.Storehouse)
            {
                return playerOneHoles[playerOneHoles.Length - 1];
            }
            else if (current.GetHoleType() == HoleType.PlayerOne)
            {
                int idx = current.GetHoleIndex();
                if (idx > 0) return playerOneHoles[idx - 1];
                else return playerTwoHoles[0];
            }
        }

        return null;
    }

    private Hole GetOppositeHole(Hole hole)
    {
        if (hole.GetHoleType() == HoleType.PlayerOne)
        {
            int idx = hole.GetHoleIndex();
            return playerTwoHoles[idx];
        }
        else if (hole.GetHoleType() == HoleType.PlayerTwo)
        {
            int idx = hole.GetHoleIndex();
            return playerOneHoles[idx];
        }
        return null;
    }

    // ========= UI SYNC =========
    private void UpdateUI()
    {
        if (uiController != null)
        {
            uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount());
            uiController.UpdateTurnUI(currentPlayer);
        }
    }
}