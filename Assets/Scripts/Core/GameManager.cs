using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    public static GameManager Instance;
    [SerializeField] public GameConfig gameConfig;
    [SerializeField] private BoardState boardState;

    [Header("Board Setup - 7 Holes Per Player")]
    [SerializeField] private Transform[] playerOneHoles = new Transform[7]; // Inspector: Hole6 left → Hole0 right
    [SerializeField] private Transform[] playerTwoHoles = new Transform[7]; // Inspector: Opponent0 left → Opponent6 right
    [SerializeField] private Transform playerOneStorehouse;
    [SerializeField] private Transform playerTwoStorehouse;

    [Header("Game State")]
    public bool isGameActive = false;
    public int currentPlayer = 1; // 1 = PlayerOne, 2 = PlayerTwo

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        bool hasError = false;
        if (playerOneHoles.Length != 7) { Debug.LogError("PlayerOneHoles must have 7 elements"); hasError = true; }
        if (playerTwoHoles.Length != 7) { Debug.LogError("PlayerTwoHoles must have 7 elements"); hasError = true; }
        if (playerOneStorehouse == null || playerTwoStorehouse == null) { Debug.LogError("Storehouses not assigned"); hasError = true; }
        if (gameConfig == null) { Debug.LogError("GameConfig missing"); hasError = true; }
        if (hasError) return;

        StartCoroutine(InitializeGameDelayed());
    }

    private IEnumerator InitializeGameDelayed()
    {
        yield return null;
        InitializeGame();
    }

    public void InitializeGame()
    {
        boardState = new BoardState
        {
            playerOneHoles = new int[7],
            playerTwoHoles = new int[7],
            playerOneProtected = new bool[7],
            playerTwoProtected = new bool[7],
            currentPlayer = 1,
            gameStatus = GameStatus.Playing
        };

        for (int i = 0; i < 7; i++)
        {
            boardState.playerOneHoles[i] = gameConfig.initialSeedsPerHole;
            boardState.playerTwoHoles[i] = gameConfig.initialSeedsPerHole;
            boardState.playerOneProtected[i] = false;
            boardState.playerTwoProtected[i] = false;
        }

        boardState.playerOneScore = 0;
        boardState.playerTwoScore = 0;
        currentPlayer = 1;
        isGameActive = true;

        UpdateBoardVisuals();
        UpdateUI();
    }

    // Map inspector index → canonical pit index
    private int GetPitIndexFromHoleIndex(int holeIndex, bool isPlayerOne)
    {
        if (isPlayerOne)
            return 6 - holeIndex; // reversed for PlayerOne
        else
            return 8 + holeIndex; // straight for PlayerTwo
    }

    public bool IsValidMove(int holeIndex)
    {
        if (!isGameActive) return false;
        if (currentPlayer == 1)
            return boardState.playerOneHoles[holeIndex] > 0 && !boardState.playerOneProtected[holeIndex];
        else
            return boardState.playerTwoHoles[holeIndex] > 0 && !boardState.playerTwoProtected[holeIndex];
    }

    public void MakeMove(int startingHoleIndex)
    {
        if (!IsValidMove(startingHoleIndex)) return;
        StartCoroutine(SowSeedsWithAnimation(startingHoleIndex));
    }

    // 🔄 Continuous Congklak sowing with capture rule
    public IEnumerator SowSeedsWithAnimation(int startingHoleIndex)
    {
        isGameActive = false;
        bool isPlayerOne = currentPlayer == 1;

        // Copy state to pits
        int[] pits = new int[16];
        for (int i = 0; i < 7; i++)
        {
            pits[i] = boardState.playerOneHoles[i];
            pits[8 + i] = boardState.playerTwoHoles[i];
        }
        pits[7] = boardState.playerOneScore;
        pits[15] = boardState.playerTwoScore;

        int pitIndex = GetPitIndexFromHoleIndex(startingHoleIndex, isPlayerOne);
        int seeds = pits[pitIndex];
        pits[pitIndex] = 0;

        // Clear visuals immediately
        if (SeedPool.Instance != null)
        {
            Hole startHole = GetHoleTransform(startingHoleIndex, isPlayerOne).GetComponent<Hole>();
            if (startHole != null)
            {
                startHole.SetSeedCount(0);
                SeedPool.Instance.UpdateHoleSeeds(startHole);
            }
        }

        int lastPit = pitIndex;

        // 🔄 Continuous sowing loop
        while (true)
        {
            while (seeds > 0)
            {
                lastPit = (lastPit + 1) % 16;

                // Skip opponent’s storehouse
                if ((isPlayerOne && lastPit == 15) || (!isPlayerOne && lastPit == 7))
                    continue;

                pits[lastPit]++;

                // Animate drop
                if (SeedPool.Instance != null)
                {
                    Transform toTransform = PitToTransform(lastPit);
                    if (toTransform != null)
                    {
                        GameObject seed = SeedPool.Instance.GetSeed();
                        seed.transform.position = toTransform.position;
                        StartCoroutine(SeedPool.Instance.AnimateSeedMovement(
                            seed, seed.transform, toTransform, 0f));
                    }
                }

                seeds--;
                yield return new WaitForSeconds(gameConfig.animationSpeed);
            }

            // 🌟 Finished sowing → check where last seed landed
            if ((isPlayerOne && lastPit == 7) || (!isPlayerOne && lastPit == 15))
            {
                // Landed in own storehouse → extra turn
                Debug.Log("Extra turn!");
                break;
            }
            else if (pits[lastPit] > 1)
            {
                // Landed in non-empty hole → continue sowing
                seeds = pits[lastPit];
                pits[lastPit] = 0;

                Hole hole = PitToTransform(lastPit)?.GetComponent<Hole>();
                if (hole != null)
                {
                    hole.SetSeedCount(0);
                    SeedPool.Instance?.UpdateHoleSeeds(hole);
                }
            }
            else
            {
                // Landed in empty hole → possible capture
                if (isPlayerOne && lastPit >= 0 && lastPit <= 6)
                {
                    int oppositePit = 14 - lastPit;
                    if (pits[oppositePit] > 0)
                    {
                        int captured = pits[oppositePit] + 1;
                        pits[oppositePit] = 0;
                        pits[lastPit] = 0;
                        pits[7] += captured;
                    }
                }
                else if (!isPlayerOne && lastPit >= 8 && lastPit <= 14)
                {
                    int oppositePit = 14 - lastPit;
                    if (pits[oppositePit] > 0)
                    {
                        int captured = pits[oppositePit] + 1;
                        pits[oppositePit] = 0;
                        pits[lastPit] = 0;
                        pits[15] += captured;
                    }
                }
                break; // stop turn
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Write back board state
        for (int i = 0; i < 7; i++)
        {
            boardState.playerOneHoles[i] = pits[i];
            boardState.playerTwoHoles[i] = pits[8 + i];
        }
        boardState.playerOneScore = pits[7];
        boardState.playerTwoScore = pits[15];

        UpdateBoardVisuals();

        // --- End of Turn ---
        if (CheckGameEnd())
        {
            EndGame();
            yield break;
        }

        // Extra turn rule
        if ((isPlayerOne && lastPit == 7) || (!isPlayerOne && lastPit == 15))
        {
            isGameActive = true;
            UpdateUI();
            yield break;
        }

        // Otherwise, switch turns
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        boardState.currentPlayer = currentPlayer;
        UpdateUI();
        isGameActive = true;
    }

    private Transform PitToTransform(int pitIndex)
    {
        if (pitIndex >= 0 && pitIndex <= 6)
            return playerOneHoles[6 - pitIndex];
        if (pitIndex == 7) return playerOneStorehouse;
        if (pitIndex >= 8 && pitIndex <= 14)
            return playerTwoHoles[pitIndex - 8];
        if (pitIndex == 15) return playerTwoStorehouse;
        return null;
    }

    private bool CheckGameEnd()
    {
        bool p1Has = false, p2Has = false;
        for (int i = 0; i < 7; i++)
        {
            if (boardState.playerOneHoles[i] > 0) p1Has = true;
            if (boardState.playerTwoHoles[i] > 0) p2Has = true;
        }
        return !p1Has || !p2Has;
    }

    private void EndGame()
    {
        isGameActive = false;

        for (int i = 0; i < 7; i++)
        {
            boardState.playerOneScore += boardState.playerOneHoles[i];
            boardState.playerTwoScore += boardState.playerTwoHoles[i];
            boardState.playerOneHoles[i] = 0;
            boardState.playerTwoHoles[i] = 0;
        }

        if (ReseedNextRound())
        {
            Debug.Log("Next round started with Ngacang rules.");
            isGameActive = true;
            UpdateBoardVisuals();
            UpdateUI();
            return;
        }

        if (boardState.playerOneScore > boardState.playerTwoScore)
        {
            boardState.gameStatus = GameStatus.PlayerOneWins;
            boardState.winner = 1;
        }
        else if (boardState.playerTwoScore > boardState.playerOneScore)
        {
            boardState.gameStatus = GameStatus.PlayerTwoWins;
            boardState.winner = 2;
        }
        else
        {
            boardState.gameStatus = GameStatus.Draw;
            boardState.winner = 0;
        }

        UpdateBoardVisuals();
        UpdateUI();
    }

    private bool ReseedNextRound()
    {
        bool reseeded = false;

        int availableSeeds1 = boardState.playerOneScore;
        int holesFilled1 = 0;
        for (int i = 0; i < 7 && availableSeeds1 > 0; i++)
        {
            if (boardState.playerOneProtected[i]) continue;
            boardState.playerOneHoles[i] = 1;
            availableSeeds1--;
            holesFilled1++;
        }
        boardState.playerOneScore = availableSeeds1;
        if (holesFilled1 < 7) boardState.playerOneProtected[6] = true;

        int availableSeeds2 = boardState.playerTwoScore;
        int holesFilled2 = 0;
        for (int i = 0; i < 7 && availableSeeds2 > 0; i++)
        {
            if (boardState.playerTwoProtected[i]) continue;
            boardState.playerTwoHoles[i] = 1;
            availableSeeds2--;
            holesFilled2++;
        }
        boardState.playerTwoScore = availableSeeds2;
        if (holesFilled2 < 7) boardState.playerTwoProtected[6] = true;

        if (holesFilled1 > 0 || holesFilled2 > 0) reseeded = true;
        return reseeded;
    }

    private Transform GetHoleTransform(int holeIndex, bool isPlayerOneSide)
    {
        return isPlayerOneSide ? playerOneHoles[holeIndex] : playerTwoHoles[holeIndex];
    }

    private void UpdateBoardVisuals()
    {
        for (int i = 0; i < 7; i++)
        {
            if (playerOneHoles[6 - i] != null)
            {
                Hole holeComp = playerOneHoles[6 - i].GetComponent<Hole>();
                if (holeComp != null)
                {
                    holeComp.SetSeedCount(boardState.playerOneHoles[i]);
                    holeComp.SetProtected(boardState.playerOneProtected[i]);
                    SeedPool.Instance?.UpdateHoleSeeds(holeComp);
                }
            }

            if (playerTwoHoles[i] != null)
            {
                Hole holeComp = playerTwoHoles[i].GetComponent<Hole>();
                if (holeComp != null)
                {
                    holeComp.SetSeedCount(boardState.playerTwoHoles[i]);
                    holeComp.SetProtected(boardState.playerTwoProtected[i]);
                    SeedPool.Instance?.UpdateHoleSeeds(holeComp);
                }
            }
        }

        if (playerOneStorehouse != null)
        {
            Hole store = playerOneStorehouse.GetComponent<Hole>();
            store?.SetSeedCount(boardState.playerOneScore);
            SeedPool.Instance?.UpdateHoleSeeds(store);
        }

        if (playerTwoStorehouse != null)
        {
            Hole store = playerTwoStorehouse.GetComponent<Hole>();
            store?.SetSeedCount(boardState.playerTwoScore);
            SeedPool.Instance?.UpdateHoleSeeds(store);
        }
    }

    private void UpdateUI()
    {
        UIController.Instance?.UpdateGameState(boardState);
    }

    public void RestartGame()
    {
        InitializeGame();
    }
}

[System.Serializable]
public class BoardState
{
    public int[] playerOneHoles;
    public int[] playerTwoHoles;
    public int playerOneScore;
    public int playerTwoScore;
    public int currentPlayer;
    public GameStatus gameStatus;
    public int winner;

    public bool[] playerOneProtected;
    public bool[] playerTwoProtected;
}

public enum GameStatus { Playing, PlayerOneWins, PlayerTwoWins, Draw }
public enum AILevel { Easy, Medium, Hard }
public enum GameMode { PlayerVsPlayer, PlayerVsAI }