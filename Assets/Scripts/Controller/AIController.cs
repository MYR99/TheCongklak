using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Helper class untuk menyimpan kondisi papan hasil simulasi
/// </summary>
public class BoardState
{

    public Dictionary<Hole, int> holeSeeds = new Dictionary<Hole, int>();

    public int p1StoreSeeds;
    public int p2StoreSeeds;

    public BoardState(BoardState original)
    {
        this.holeSeeds = new Dictionary<Hole, int>(original.holeSeeds);
        this.p1StoreSeeds = original.p1StoreSeeds;
        this.p2StoreSeeds = original.p2StoreSeeds;
    }

    public BoardState() { }
}

/// <summary>
/// Helper struct untuk menyimpan info hasil gerakan
/// </summary>
public struct MoveInfo
{
    public Hole Move;        
    public int ScoreGained;    
    public bool IsExtraTurn;  
    public BoardState NextBoardState; 
}


public class AIController : MonoBehaviour
{
    public static AIController Instance { get; private set; }

    private GameManager gameManager;
    private Hole[] aiHoles;        
    private Hole[] playerHoles;    
    private Hole aiStorehouse;   
    private Hole playerStorehouse;
    
    private int totalBoardHoles;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// DIPERBARUI
    /// </summary>
    public void Initialize(GameManager gm, Hole[] p2Holes, Hole[] p1Holes, Hole p2Storehouse, Hole p1Storehouse)
    {
        gameManager = gm;
        aiHoles = p2Holes;
        playerHoles = p1Holes;
        aiStorehouse = p2Storehouse;
        playerStorehouse = p1Storehouse;
        totalBoardHoles = aiHoles.Length + playerHoles.Length;
    }

    public void MakeMove(AILevel difficulty)
    {
        StartCoroutine(MakeMoveCoroutine(difficulty));
    }

    private IEnumerator MakeMoveCoroutine(AILevel difficulty)
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 1.75f));

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
            gameManager.EndTurnAfterAI(); 
        }
    }

    /// <summary>
    /// Mendapatkan daftar lubang yang bisa dimainkan (ada bijinya)
    /// </summary>
    private List<Hole> GetValidMoves(BoardState board, int player)
    {
        Hole[] holesToCheck = (player == 1) ? playerHoles : aiHoles;
        return holesToCheck.Where(h => board.holeSeeds[h] > 0).ToList();
    }

    /// <summary>
    /// AI EASY
    /// </summary>
    private Hole FindBestMove_Easy()
    {
        BoardState currentBoard = CreateCurrentBoardState();
        var validMoves = GetValidMoves(currentBoard, 2); // 2 = AI
        if (validMoves.Count == 0) return null;

        foreach (var move in validMoves)
        {
            MoveInfo info = SimulateMove(currentBoard, move, 2);
            if (info.IsExtraTurn)
            {
                return move; 
            }
        }

        return validMoves[Random.Range(0, validMoves.Count)];
    }

    /// <summary>
    /// AI MEDIUM
    /// </summary>
    private Hole FindBestMove_Medium()
    {
        BoardState currentBoard = CreateCurrentBoardState();
        var validMoves = GetValidMoves(currentBoard, 2); 
        if (validMoves.Count == 0) return null;

        Hole bestMove = null;
        int bestScore = -1; 

        foreach (var move in validMoves)
        {
            MoveInfo info = SimulateMove(currentBoard, move, 2);
            int currentMoveScore = 0;

            if (info.IsExtraTurn)
            {
                currentMoveScore = 100; 
            }
            else
            {
                currentMoveScore = info.ScoreGained; 
            }

            if (currentMoveScore > bestScore)
            {
                bestScore = currentMoveScore;
                bestMove = move;
            }
        }

        if (bestScore <= 0)
        {
            return validMoves[Random.Range(0, validMoves.Count)];
        }

        return bestMove;
    }

    /// <summary>
    /// AI HARD
    /// </summary>
    private Hole FindBestMove_Hard()
    {
        BoardState currentBoard = CreateCurrentBoardState();
        var validMoves = GetValidMoves(currentBoard, 2); 
        if (validMoves.Count == 0) return null;

        Hole bestMove = null;
        int bestNetScore = int.MinValue; 

        foreach (var move in validMoves)
        {
            MoveInfo aiMoveInfo = SimulateMove(currentBoard, move, 2);
            if (aiMoveInfo.IsExtraTurn)
            {
                int currentScore = 1000 + aiMoveInfo.ScoreGained; 
                if (currentScore > bestNetScore)
                {
                    bestNetScore = currentScore;
                    bestMove = move;
                }
            }
        }

        if (bestMove != null)
        {
            return bestMove;
        }

        bestNetScore = int.MinValue;

        foreach (var move in validMoves)
        {

            MoveInfo aiMoveInfo = SimulateMove(currentBoard, move, 2);
            int aiScore = aiMoveInfo.ScoreGained;

            int playerBestReplyScore = 0;
            var playerValidReplies = GetValidMoves(aiMoveInfo.NextBoardState, 1); 

            foreach (var playerMove in playerValidReplies)
            {
                MoveInfo playerReplyInfo = SimulateMove(aiMoveInfo.NextBoardState, playerMove, 1);
                
                int replyScore = playerReplyInfo.ScoreGained;
                if (playerReplyInfo.IsExtraTurn)
                {
                    replyScore += 50; 
                }
                
                if (replyScore > playerBestReplyScore)
                {
                    playerBestReplyScore = replyScore;
                }
            }

            int netScore = aiScore - playerBestReplyScore;

            if (netScore > bestNetScore)
            {
                bestNetScore = netScore;
                bestMove = move;
            }
        }

        if (bestMove == null && validMoves.Count > 0)
        {
            bestMove = validMoves[Random.Range(0, validMoves.Count)];
        }

        return bestMove;
    }

    /// <summary>
    /// Mensimulasikan seluruh gerakan, termasuk "tembakan" dan giliran berulang
    /// </summary>
    private MoveInfo SimulateMove(BoardState board, Hole startHole, int player)
    {
        BoardState newState = new BoardState(board);
        
        MoveInfo info = new MoveInfo
        {
            Move = startHole,
            ScoreGained = 0,
            IsExtraTurn = false
        };

        Hole currentHole = startHole;
        int seedsInHand = newState.holeSeeds[startHole];
        newState.holeSeeds[startHole] = 0; 

        while (seedsInHand > 0)
        {
            currentHole = gameManager.GetNextHolePublic(currentHole);

            if (player == 1 && currentHole == aiStorehouse) continue;
            if (player == 2 && currentHole == playerStorehouse) continue;

            seedsInHand--;

            if (currentHole == playerStorehouse)
            {
                newState.p1StoreSeeds++;
            }
            else if (currentHole == aiStorehouse)
            {
                newState.p2StoreSeeds++;
            }
            else 
            {
                newState.holeSeeds[currentHole]++;
            }

            if (seedsInHand == 0)
            {

                if (player == 1 && currentHole == playerStorehouse) info.IsExtraTurn = true;
                if (player == 2 && currentHole == aiStorehouse) info.IsExtraTurn = true;

                if (!info.IsExtraTurn && newState.holeSeeds[currentHole] == 1)
                {
                    bool belongsToPlayer = (player == 1 && currentHole.GetHoleType() == HoleType.PlayerOne) ||
                                           (player == 2 && currentHole.GetHoleType() == HoleType.PlayerTwo);

                    if (belongsToPlayer)
                    {
                        Hole oppositeHole = GetOppositeHole(currentHole);
                        if (oppositeHole != null && newState.holeSeeds[oppositeHole] > 0)
                        {
                            int captured = newState.holeSeeds[oppositeHole] + 1; 
                            newState.holeSeeds[oppositeHole] = 0;
                            newState.holeSeeds[currentHole] = 0;
                            
                            if (player == 1) newState.p1StoreSeeds += captured;
                            else newState.p2StoreSeeds += captured;
                            
                            info.ScoreGained = captured;
                        }
                    }
                }

                if (!info.IsExtraTurn && currentHole.GetHoleType() != HoleType.Storehouse && newState.holeSeeds[currentHole] > 1)
                {
                    seedsInHand = newState.holeSeeds[currentHole];
                    newState.holeSeeds[currentHole] = 0;
                }
            }
        }
        
        info.NextBoardState = newState;
        return info;
    }

    /// <summary>
    /// Membuat snapshot kondisi papan saat ini dari game
    /// </summary>
    private BoardState CreateCurrentBoardState()
    {
        BoardState state = new BoardState();
        foreach (var hole in playerHoles) state.holeSeeds[hole] = hole.GetSeedCount();
        foreach (var hole in aiHoles) state.holeSeeds[hole] = hole.GetSeedCount();
        state.p1StoreSeeds = playerStorehouse.GetSeedCount();
        state.p2StoreSeeds = aiStorehouse.GetSeedCount();
        return state;
    }

    /// <summary>
    /// Helper untuk mendapatkan lubang seberang
    /// </summary>
    private Hole GetOppositeHole(Hole hole)
    {
        int index = hole.GetHoleIndex();
        if (hole.GetHoleType() == HoleType.PlayerOne)
        {
            return aiHoles[totalBoardHoles / 2 - 1 - index];
        }
        else if (hole.GetHoleType() == HoleType.PlayerTwo)
        {
            return playerHoles[totalBoardHoles / 2 - 1 - index];
        }
        return null;
    }
}