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

    [Header("Config")]
    public int seedsPerHole = 7;

    // --- Game State ---
    public int CurrentPlayer { get; private set; } = 1; // 1 = PlayerOne, 2 = PlayerTwo
    private bool isGameActive = false;
    public bool IsGameActive => isGameActive; // read-only property

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        // Reset holes
        foreach (var hole in playerOneHoles) hole.SetSeeds(seedsPerHole);
        foreach (var hole in playerTwoHoles) hole.SetSeeds(seedsPerHole);

        playerOneStorehouse.SetSeeds(0);
        playerTwoStorehouse.SetSeeds(0);

        CurrentPlayer = 1;
        isGameActive = true;

        Debug.Log("🎮 Game dimulai, giliran Player One");
    }

    // --- Restart game untuk dipanggil dari UI ---
    public void RestartGame()
    {
        StartNewGame();
    }

    // --- Validasi move ---
    public bool IsValidMove(Hole hole)
    {
        if (!isGameActive) return false;
        if (hole.GetSeedCount() == 0) return false;

        if (CurrentPlayer == 1 && hole.GetHoleType() == HoleType.PlayerOne)
            return true;
        if (CurrentPlayer == 2 && hole.GetHoleType() == HoleType.PlayerTwo)
            return true;

        return false;
    }

    // --- Main sowing routine ---
    public IEnumerator SowSeeds(Hole startHole, int seeds)
    {
        Debug.Log($"🌱 Menyebar {seeds} biji dari {startHole.name}");
        Hole currentHole = startHole;

        while (seeds > 0)
        {
            currentHole = GetNextHole(currentHole);

            // skip storehouse lawan
            if ((CurrentPlayer == 1 && currentHole == playerTwoStorehouse) ||
                (CurrentPlayer == 2 && currentHole == playerOneStorehouse))
            {
                continue;
            }

            currentHole.AddSeed();
            seeds--;

            yield return new WaitForSeconds(0.2f);
        }

        HandleTurnEnd(currentHole);
        CheckGameOver();
    }

    // --- Aturan setelah sowing ---
    private void HandleTurnEnd(Hole lastHole)
    {
        // 1. Extra turn kalau terakhir jatuh di storehouse sendiri
        if ((CurrentPlayer == 1 && lastHole == playerOneStorehouse) ||
            (CurrentPlayer == 2 && lastHole == playerTwoStorehouse))
        {
            Debug.Log("🔄 Extra turn!");
            return;
        }

        // 2. Capture kalau terakhir jatuh di lubang kosong milik sendiri
        if (lastHole.GetSeedCount() == 1)
        {
            if (CurrentPlayer == 1 && lastHole.GetHoleType() == HoleType.PlayerOne)
            {
                int idx = lastHole.GetHoleIndex();
                int captured = playerTwoHoles[idx].TakeAllSeeds();
                if (captured > 0)
                {
                    playerOneStorehouse.SetSeeds(playerOneStorehouse.GetSeedCount() + captured + 1);
                    lastHole.TakeAllSeeds();
                    Debug.Log($"🎯 PlayerOne capture {captured} biji!");
                }
            }
            else if (CurrentPlayer == 2 && lastHole.GetHoleType() == HoleType.PlayerTwo)
            {
                int idx = lastHole.GetHoleIndex();
                int captured = playerOneHoles[idx].TakeAllSeeds();
                if (captured > 0)
                {
                    playerTwoStorehouse.SetSeeds(playerTwoStorehouse.GetSeedCount() + captured + 1);
                    lastHole.TakeAllSeeds();
                    Debug.Log($"🎯 PlayerTwo capture {captured} biji!");
                }
            }
        }

        // 3. Ganti giliran
        CurrentPlayer = (CurrentPlayer == 1) ? 2 : 1;
        Debug.Log($"👉 Sekarang giliran Player {CurrentPlayer}");
    }

    // --- Cek akhir permainan ---
    private void CheckGameOver()
    {
        bool playerOneEmpty = true;
        bool playerTwoEmpty = true;

        foreach (var hole in playerOneHoles)
        {
            if (hole.GetSeedCount() > 0) { playerOneEmpty = false; break; }
        }

        foreach (var hole in playerTwoHoles)
        {
            if (hole.GetSeedCount() > 0) { playerTwoEmpty = false; break; }
        }

        if (playerOneEmpty || playerTwoEmpty)
        {
            // pindahkan semua biji yang tersisa ke storehouse masing-masing
            int p1Extra = 0, p2Extra = 0;

            foreach (var hole in playerOneHoles) p1Extra += hole.TakeAllSeeds();
            foreach (var hole in playerTwoHoles) p2Extra += hole.TakeAllSeeds();

            playerOneStorehouse.SetSeeds(playerOneStorehouse.GetSeedCount() + p1Extra);
            playerTwoStorehouse.SetSeeds(playerTwoStorehouse.GetSeedCount() + p2Extra);

            isGameActive = false;

            // tentukan pemenang
            int p1Score = playerOneStorehouse.GetSeedCount();
            int p2Score = playerTwoStorehouse.GetSeedCount();

            if (p1Score > p2Score) Debug.Log($"🏆 Player One menang! {p1Score} vs {p2Score}");
            else if (p2Score > p1Score) Debug.Log($"🏆 Player Two menang! {p2Score} vs {p1Score}");
            else Debug.Log($"🤝 Seri! {p1Score} vs {p2Score}");
        }
    }

    // --- Navigasi hole berikutnya (searah jarum jam) ---
    private Hole GetNextHole(Hole current)
    {
        if (current.GetHoleType() == HoleType.PlayerOne)
        {
            int idx = current.GetHoleIndex();
            if (idx < playerOneHoles.Length - 1) return playerOneHoles[idx + 1];
            else return playerOneStorehouse;
        }
        else if (current.GetHoleType() == HoleType.PlayerTwo)
        {
            int idx = current.GetHoleIndex();
            if (idx < playerTwoHoles.Length - 1) return playerTwoHoles[idx + 1];
            else return playerTwoStorehouse;
        }
        else if (current == playerOneStorehouse)
        {
            return playerTwoHoles[0];
        }
        else if (current == playerTwoStorehouse)
        {
            return playerOneHoles[0];
        }

        return playerOneHoles[0]; // fallback
    }
}