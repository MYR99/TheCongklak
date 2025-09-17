using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Board Setup - 7 Holes Per Player")]
    public Hole[] playerOneHoles;
    public Hole[] playerTwoHoles;
    public Hole playerOneStorehouse;
    public Hole playerTwoStorehouse;

    [Header("UI")]
    public UIController uiController;

    [Header("Config")]
    public int seedsPerHole = 7; // jumlah awal biji per lubang

    [HideInInspector] public bool isPlayerOneTurn = true;
    [HideInInspector] public bool isGameActive = true;

    private BoardState boardState;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RestartGame();
    }

    // ✅ Restart game dipanggil dari UI
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        InitializeBoard();
        isPlayerOneTurn = true;
        isGameActive = true;

        if (uiController != null)
        {
            uiController.UpdateBoardUI(boardState);
            uiController.UpdateTurnIndicator(isPlayerOneTurn);
        }
    }
    // ✅ Reset papan
    private void InitializeBoard()
    {
        boardState = new BoardState();

        // isi lubang player one
        for (int i = 0; i < playerOneHoles.Length; i++)
        {
            playerOneHoles[i].SetSeeds(seedsPerHole);
            boardState.playerOneHoles[i] = seedsPerHole;
        }
        // isi lubang player two
        for (int i = 0; i < playerTwoHoles.Length; i++)
        {
            playerTwoHoles[i].SetSeeds(seedsPerHole);
            boardState.playerTwoHoles[i] = seedsPerHole;
        }
        // kosongkan storehouse
        playerOneStorehouse.SetSeeds(0);
        playerTwoStorehouse.SetSeeds(0);

        boardState.playerOneScore = 0;
        boardState.playerTwoScore = 0;
    }
    // ✅ Cek akhir permainan
    private bool CheckGameEnd()
    {
        int totalSeeds = boardState.playerOneScore + boardState.playerTwoScore;

        // selesai kalau semua biji sudah masuk storehouse
        if (totalSeeds >= (seedsPerHole * 14))
        {
            return true;
        }
        // selesai kalau salah satu sisi habis
        bool p1Has = false, p2Has = false;
        for (int i = 0; i < 7; i++)
        {
            if (boardState.playerOneHoles[i] > 0) p1Has = true;
            if (boardState.playerTwoHoles[i] > 0) p2Has = true;
        }
        return !p1Has || !p2Has;
    }

    // ✅ Selesaikan game
    private void EndGame()
    {
        isGameActive = false;

        string result = "";
        if (boardState.playerOneScore > boardState.playerTwoScore)
            result = "Player One Wins!";
        else if (boardState.playerOneScore < boardState.playerTwoScore)
            result = "Player Two Wins!";
        else
            result = "Draw!";

        Debug.Log("Game Over: " + result);

        if (uiController != null)
            uiController.ShowGameOver(result);
    }
    // ✅ Dipanggil setelah setiap sowing
    public void OnTurnFinished()
    {
        if (CheckGameEnd())
        {
            EndGame();
            return;
        }

        isPlayerOneTurn = !isPlayerOneTurn;

        if (uiController != null)
            uiController.UpdateTurnIndicator(isPlayerOneTurn);
    }

    // ✅ Validasi move
    public bool IsValidMove(int holeIndex)
    {
        if (!isGameActive) return false;

        if (isPlayerOneTurn)
        {
            if (holeIndex >= 0 && holeIndex < playerOneHoles.Length)
                return playerOneHoles[holeIndex].GetSeedCount() > 0;
        }
        else
        {
            if (holeIndex >= 0 && holeIndex < playerTwoHoles.Length)
                return playerTwoHoles[holeIndex].GetSeedCount() > 0;
        }

        return false;
    }
}