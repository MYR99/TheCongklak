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

    public void Initialize(GameManager gm, Hole[] p2Holes, Hole[] p1Holes, Hole p2Storehouse)
    {
        gameManager = gm;
        aiHoles = p2Holes;
        playerHoles = p1Holes;
        aiStorehouse = p2Storehouse;
    }

    public void MakeMove(AILevel difficulty)
    {
        StartCoroutine(MakeMoveCoroutine(difficulty));
    }

    private IEnumerator MakeMoveCoroutine(AILevel difficulty)
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));

        Hole selectedHole = null;
        switch (difficulty)
        {
            case AILevel.Easy:
                selectedHole = FindBestMove_Easy();
                break;
            case AILevel.Medium:
                selectedHole = FindBestMove_Medium();
                break;
            case AILevel.Hard:
                selectedHole = FindBestMove_Hard();
                break;
        }

        if (selectedHole != null)
        {
            gameManager.SelectHole(selectedHole);
        }
        else
        {
            Debug.LogWarning("AI could not find a valid move.");
            gameManager.EndTurnAfterAI();
        }
    }

    private List<Hole> GetValidMoves()
    {
        return aiHoles.Where(h => h.GetSeedCount() > 0).ToList();
    }

    private Hole FindBestMove_Easy()
    {
        List<Hole> validMoves = GetValidMoves();
        if (validMoves.Count > 0)
        {
            return validMoves[Random.Range(0, validMoves.Count)];
        }
        return null;
    }

    private Hole FindBestMove_Medium()
    {
        List<Hole> validMoves = GetValidMoves();
        foreach (var hole in validMoves)
        {
            int seedCount = hole.GetSeedCount();
            int holeIndex = hole.GetHoleIndex();
            int totalPlayerHoles = playerHoles.Length;
            int totalAIHoles = aiHoles.Length;

            if (seedCount % (totalPlayerHoles + totalAIHoles + 1) == (totalAIHoles - holeIndex))
            {
                return hole; 
            }
        }
        return FindBestMove_Easy();
    }

    private Hole FindBestMove_Hard()
    {
        Hole extraTurnMove = FindBestMove_Medium();
        if (extraTurnMove != null && IsExtraTurn(extraTurnMove))
        {
            return extraTurnMove;
        }

        List<Hole> validMoves = GetValidMoves();
        Hole bestCaptureMove = null;
        int maxCapture = 0;

        foreach (var hole in validMoves)
        {
            Hole lastHole = SimulateSow(hole);

            if (lastHole.GetHoleType() == HoleType.PlayerTwo && lastHole.GetSeedCount() == 0) // Mendarat di lubang kosong milik AI
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

        return FindBestMove_Easy();
    }

    private bool IsExtraTurn(Hole hole)
    {
        int seedCount = hole.GetSeedCount();
        int holeIndex = hole.GetHoleIndex();
        return (seedCount % (playerHoles.Length + aiHoles.Length + 1) == (aiHoles.Length - holeIndex));
    }

    private Hole GetOppositeHole(int aiHoleIndex)
    {
        int oppositeIndex = playerHoles.Length - 1 - aiHoleIndex;
        if (oppositeIndex >= 0 && oppositeIndex < playerHoles.Length)
        {
            return playerHoles[oppositeIndex];
        }
        return null;
    }
    
    private Hole SimulateSow(Hole startHole)
    {
        int seedCount = startHole.GetSeedCount();
        Hole currentHole = startHole;

        for (int i = 0; i < seedCount; i++)
        {
            currentHole = gameManager.GetNextHolePublic(currentHole);

            if (currentHole.GetHoleType() == HoleType.Storehouse && currentHole.GetHoleType() != aiStorehouse.GetHoleType())
            {
                currentHole = gameManager.GetNextHolePublic(currentHole);
            }
        }
        return currentHole;
    }
}