using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : MonoBehaviour
{
    public static AIController Instance { get; private set; }

    private GameManager gameManager;
    private Hole[] aiHoles;
    private Hole[] playerHoles;
    private Hole aiStorehouse;
    private Hole playerStorehouse;

    private int totalSowableHoles;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Diperbarui
    /// </summary>
    public void Initialize(GameManager gm, Hole[] p2Holes, Hole[] p1Holes, Hole p2Storehouse, Hole p1Storehouse)
    {
        gameManager = gm;
        aiHoles = p2Holes;
        playerHoles = p1Holes;
        aiStorehouse = p2Storehouse;
        playerStorehouse = p1Storehouse; 

        totalSowableHoles = playerHoles.Length + aiHoles.Length + 1;
    }

    public void MakeMove(AILevel difficulty)
    {
        StartCoroutine(MakeMoveCoroutine(difficulty));
    }

    private IEnumerator MakeMoveCoroutine(AILevel difficulty)
    {
        yield return new WaitForSeconds(Random.Range(1f, 1.5f));

        float randomChance = Random.Range(0f, 1f); // Angka acak 0.0 - 1.0
        Hole selectedHole = null;

        switch (difficulty)
        {
            case AILevel.Easy:
 
                if (randomChance < 0.9f)
                {
                    selectedHole = FindBestMove_Random();
                }
                else
                {
                    selectedHole = FindBestMove_MediumLogic();
                }
                break;

            case AILevel.Medium:

                if (randomChance < 0.5f)
                {
                    selectedHole = FindBestMove_Random();
                }
                else
                {
                    selectedHole = FindBestMove_MediumLogic();
                }
                break;

            case AILevel.Hard:

                if (randomChance < 0.4f)
                {
                    selectedHole = FindBestMove_Random();
                }
                else
                {
                    selectedHole = FindBestMove_HardLogic();
                }
                break;
        }

        if (selectedHole == null)
        {
            selectedHole = FindBestMove_Random();
        }

        if (selectedHole != null)
        {
            gameManager.SelectHole(selectedHole);
        }
        else
        {
            gameManager.EndTurnAfterAI();
        }
    }

    private List<Hole> GetValidMoves()
    {
        return aiHoles.Where(h => h.GetSeedCount() > 0).ToList();
    }

    /// <summary>
    /// LOGIKA 1: Acak Murni (Easy)
    /// </summary>
    private Hole FindBestMove_Random()
    {
        List<Hole> validMoves = GetValidMoves();
        if (validMoves.Count > 0)
        {
            return validMoves[Random.Range(0, validMoves.Count)];
        }
        return null;
    }

    /// <summary>
    /// LOGIKA 2: Mencari Giliran Tambahan (Medium)
    /// </summary>
    private Hole FindBestMove_MediumLogic()
    {
        List<Hole> validMoves = GetValidMoves();
        if (validMoves.Count == 0) return null;

        foreach (var hole in validMoves)
        {
            if (IsExtraTurn(hole))
            {
                return hole; 
            }
        }

        return FindBestMove_Random();
    }

    /// <summary>
    /// LOGIKA 3: Mencari Giliran Tambahan + Tembakan Terbaik (Hard)
    /// </summary>
    private Hole FindBestMove_HardLogic()
    {

        Hole extraTurnMove = FindBestMove_MediumLogic();
        if (extraTurnMove != null && IsExtraTurn(extraTurnMove)) 
        {
            return extraTurnMove;
        }

        List<Hole> validMoves = GetValidMoves();
        if (validMoves.Count == 0) return null;

        Hole bestCaptureMove = null;
        int maxCapture = 0;

        foreach (var hole in validMoves)
        {

            Hole lastHole = SimulateSow(hole);

            if (lastHole != null && lastHole.GetHoleType() == HoleType.PlayerTwo && lastHole.GetSeedCount() == 0) 
            {
                Hole oppositeHole = GetOppositeHole(lastHole.GetHoleIndex());
                if (oppositeHole != null && oppositeHole.GetSeedCount() > 0)
                {

                    int currentCapture = oppositeHole.GetSeedCount() + 1; 
                    if (currentCapture > maxCapture)
                    {
                        maxCapture = currentCapture;
                        bestCaptureMove = hole;
                    }
                }
            }
        }

        if (bestCaptureMove != null)
        {
            return bestCaptureMove;
        }

        return FindBestMove_Random();
    }

    /// <summary>
    /// Helper untuk mengecek apakah sebuah gerakan akan menghasilkan giliran tambahan
    /// </summary>
    private bool IsExtraTurn(Hole hole)
    {
        int seedCount = hole.GetSeedCount();
        int holeIndex = hole.GetHoleIndex();
        int distanceToStore = aiHoles.Length - holeIndex;

        return (seedCount % totalSowableHoles == distanceToStore);
    }

    /// <summary>
    /// Helper untuk mendapatkan lubang di seberang
    /// </summary>
    private Hole GetOppositeHole(int aiHoleIndex)
    {
        int oppositeIndex = playerHoles.Length - 1 - aiHoleIndex;
        if (oppositeIndex >= 0 && oppositeIndex < playerHoles.Length)
        {
            return playerHoles[oppositeIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Helper untuk simulasi sederhana mencari lubang terakhir
    /// </summary>
    private Hole SimulateSow(Hole startHole)
    {
        int seedCount = startHole.GetSeedCount();
        Hole currentHole = startHole;

        for (int i = 0; i < seedCount; i++)
        {
            currentHole = gameManager.GetNextHolePublic(currentHole);

            if (currentHole == playerStorehouse)
            {
                currentHole = gameManager.GetNextHolePublic(currentHole);
            }
        }
        return currentHole;
    }
}