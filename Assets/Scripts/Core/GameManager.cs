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
    [SerializeField] private Transform[] playerOneHoles = new Transform[7]; // Changed to 7 holes
    [SerializeField] private Transform[] playerTwoHoles = new Transform[7]; // Changed to 7 holes
    [SerializeField] private Transform playerOneStorehouse;
    [SerializeField] private Transform playerTwoStorehouse;
    
    [Header("Game State")]
    public bool isGameActive = false;
    public int currentPlayer = 1; // 1 for Player 1, 2 for Player 2
    
    private void Awake()
    {
        // Singleton pattern implementation - REMOVED DontDestroyOnLoad to fix SeedPool errors
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Wait a frame to ensure SeedPool is initialized
        StartCoroutine(InitializeGameDelayed());
    }
    
    private IEnumerator InitializeGameDelayed()
    {
        yield return null; // Wait one frame
        InitializeGame();
    }
    
    /// <summary>
    /// Initialize the game board with starting seed configuration
    /// </summary>
    public void InitializeGame()
    {
        // Initialize board state for 7 holes per player
        boardState = new BoardState();
        boardState.playerOneHoles = new int[7]; // 7 holes per player
        boardState.playerTwoHoles = new int[7]; // 7 holes per player
        
        // Set initial seeds (typically 7 seeds per hole)
        for (int i = 0; i < 7; i++) // Changed to 7 holes
        {
            boardState.playerOneHoles[i] = gameConfig.initialSeedsPerHole;
            boardState.playerTwoHoles[i] = gameConfig.initialSeedsPerHole;
        }
        
        boardState.playerOneScore = 0;
        boardState.playerTwoScore = 0;
        boardState.currentPlayer = 1;
        boardState.gameStatus = GameStatus.Playing;
        
        currentPlayer = 1;
        isGameActive = true;
        
        // Update visual representation
        UpdateBoardVisuals();
        UpdateUI();
        
        Debug.Log("Indonesian Congklak game initialized successfully with 7 holes per player!");
    }
    
    /// <summary>
    /// Check if a move is valid for the current player
    /// </summary>
    public bool IsValidMove(int holeIndex)
    {
        if (!isGameActive) return false;
        
        if (currentPlayer == 1)
        {
            return holeIndex >= 0 && holeIndex < 7 && boardState.playerOneHoles[holeIndex] > 0; // Changed to 7
        }
        else
        {
            return holeIndex >= 0 && holeIndex < 7 && boardState.playerTwoHoles[holeIndex] > 0; // Changed to 7
        }
    }
    
    /// <summary>
    /// Execute a move starting from the specified hole
    /// </summary>
    public void MakeMove(int startingHoleIndex)
    {
        if (!IsValidMove(startingHoleIndex)) return;
        
        StartCoroutine(SowSeedsWithAnimation(startingHoleIndex));
    }
    
    /// <summary>
    /// Enhanced sowing with visual animation for 7-hole Congklak
    /// </summary>
    public IEnumerator SowSeedsWithAnimation(int startingHoleIndex)
    {
        isGameActive = false;
        
        int seeds;
        int currentHole = startingHoleIndex;
        bool isPlayerOneSide = currentPlayer == 1;
        
        // Get starting hole transform for animation
        Transform startingHoleTransform = GetHoleTransform(startingHoleIndex, isPlayerOneSide);
        
        // Pick up seeds from starting hole
        if (isPlayerOneSide)
        {
            seeds = boardState.playerOneHoles[startingHoleIndex];
            boardState.playerOneHoles[startingHoleIndex] = 0;
        }
        else
        {
            seeds = boardState.playerTwoHoles[startingHoleIndex];
            boardState.playerTwoHoles[startingHoleIndex] = 0;
        }
        
        // Update visual immediately for starting hole
        UpdateHoleVisual(startingHoleIndex, isPlayerOneSide);
        
        // Create temporary seeds for animation only if SeedPool exists
        List<GameObject> animatingSeeds = new List<GameObject>();
        if (SeedPool.Instance != null)
        {
            for (int i = 0; i < seeds; i++)
            {
                GameObject seed = SeedPool.Instance.GetSeed();
                seed.transform.position = startingHoleTransform.position;
                animatingSeeds.Add(seed);
            }
        }
        
        int seedIndex = 0;
        
        // Distribute seeds with animation (7-hole version)
        while (seeds > 0)
        {
            // Move to next hole
            if (isPlayerOneSide)
            {
                currentHole++;
                if (currentHole >= 7) // Changed to 7 holes
                {
                    // Reached player's storehouse
                    if (currentPlayer == 1)
                    {
                        Transform storehouseTransform = playerOneStorehouse;
                        
                        // Animate seed to storehouse if SeedPool exists
                        if (SeedPool.Instance != null && seedIndex < animatingSeeds.Count)
                        {
                            StartCoroutine(SeedPool.Instance.AnimateSeedMovement(
                                animatingSeeds[seedIndex], 
                                animatingSeeds[seedIndex].transform, 
                                storehouseTransform,
                                seedIndex * 0.1f
                            ));
                        }
                        
                        boardState.playerOneScore++;
                        seeds--;
                        seedIndex++;
                        
                        yield return new WaitForSeconds(gameConfig.animationSpeed);
                        UpdateUI();
                        
                        if (seeds == 0) break;
                    }
                    
                    isPlayerOneSide = false;
                    currentHole = 6; // Start from rightmost hole on opponent's side (index 6)
                }
                else
                {
                    Transform targetHole = GetHoleTransform(currentHole, true);
                    
                    // Animate seed to hole if SeedPool exists
                    if (SeedPool.Instance != null && seedIndex < animatingSeeds.Count)
                    {
                        StartCoroutine(SeedPool.Instance.AnimateSeedMovement(
                            animatingSeeds[seedIndex],
                            animatingSeeds[seedIndex].transform, 
                            targetHole,
                            seedIndex * 0.1f
                        ));
                    }
                    
                    boardState.playerOneHoles[currentHole]++;
                    seeds--;
                    seedIndex++;
                    
                    yield return new WaitForSeconds(gameConfig.animationSpeed);
                    UpdateHoleVisual(currentHole, true);
                }
            }
            else
            {
                currentHole--;
                if (currentHole < 0)
                {
                    // Reached player's storehouse
                    if (currentPlayer == 2)
                    {
                        Transform storehouseTransform = playerTwoStorehouse;
                        
                        // Animate seed to storehouse if SeedPool exists
                        if (SeedPool.Instance != null && seedIndex < animatingSeeds.Count)
                        {
                            StartCoroutine(SeedPool.Instance.AnimateSeedMovement(
                                animatingSeeds[seedIndex], 
                                animatingSeeds[seedIndex].transform,
                                storehouseTransform,
                                seedIndex * 0.1f
                            ));
                        }
                        
                        boardState.playerTwoScore++;
                        seeds--;
                        seedIndex++;
                        
                        yield return new WaitForSeconds(gameConfig.animationSpeed);
                        UpdateUI();
                        
                        if (seeds == 0) break;
                    }
                    
                    isPlayerOneSide = true;
                    currentHole = 0; // Start from leftmost hole on opponent's side
                }
                else
                {
                    Transform targetHole = GetHoleTransform(currentHole, false);
                    
                    // Animate seed to hole if SeedPool exists
                    if (SeedPool.Instance != null && seedIndex < animatingSeeds.Count)
                    {
                        StartCoroutine(SeedPool.Instance.AnimateSeedMovement(
                            animatingSeeds[seedIndex],
                            animatingSeeds[seedIndex].transform,
                            targetHole,
                            seedIndex * 0.1f
                        ));
                    }
                    
                    boardState.playerTwoHoles[currentHole]++;
                    seeds--;
                    seedIndex++;
                    
                    yield return new WaitForSeconds(gameConfig.animationSpeed);
                    UpdateHoleVisual(currentHole, false);
                }
            }
        }
        
        // Wait for all animations to complete
        yield return new WaitForSeconds(1f);
        
        // Return animated seeds to pool if SeedPool exists
        if (SeedPool.Instance != null)
        {
            SeedPool.Instance.ReturnSeeds(animatingSeeds);
        }
        
        // Check for capture (Indonesian rules)
        yield return StartCoroutine(CheckAndExecuteCapture(currentHole, isPlayerOneSide));
        
        // Check for game end
        if (CheckGameEnd())
        {
            EndGame();
        }
        else
        {
            // Switch turns (unless last seed landed in own storehouse)
            bool extraTurn = false;
            if (isPlayerOneSide && currentPlayer == 1 && currentHole == -1) extraTurn = true;
            if (!isPlayerOneSide && currentPlayer == 2 && currentHole == 7) extraTurn = true; // Changed to 7
            
            if (!extraTurn)
            {
                currentPlayer = currentPlayer == 1 ? 2 : 1;
                boardState.currentPlayer = currentPlayer;
            }
            
            UpdateUI();
            isGameActive = true;
        }
    }
    
    /// <summary>
    /// Check and execute capture rules (Indonesian Congklak rules)
    /// </summary>
    private IEnumerator CheckAndExecuteCapture(int lastHole, bool isPlayerOneSide)
    {
        // Capture only if last seed lands in own empty hole
        if (isPlayerOneSide && currentPlayer == 1)
        {
            if (lastHole >= 0 && lastHole < 7 && boardState.playerOneHoles[lastHole] == 1) // Changed to 7
            {
                int oppositeHole = 6 - lastHole; // Opposite hole calculation for 7 holes
                if (boardState.playerTwoHoles[oppositeHole] > 0)
                {
                    // Execute capture
                    boardState.playerOneScore += boardState.playerOneHoles[lastHole] + boardState.playerTwoHoles[oppositeHole];
                    boardState.playerOneHoles[lastHole] = 0;
                    boardState.playerTwoHoles[oppositeHole] = 0;
                    
                    yield return new WaitForSeconds(gameConfig.animationSpeed);
                    UpdateBoardVisuals();
                    UpdateUI();
                }
            }
        }
        else if (!isPlayerOneSide && currentPlayer == 2)
        {
            if (lastHole >= 0 && lastHole < 7 && boardState.playerTwoHoles[lastHole] == 1) // Changed to 7
            {
                int oppositeHole = 6 - lastHole; // Opposite hole calculation for 7 holes
                if (boardState.playerOneHoles[oppositeHole] > 0)
                {
                    // Execute capture
                    boardState.playerTwoScore += boardState.playerTwoHoles[lastHole] + boardState.playerOneHoles[oppositeHole];
                    boardState.playerTwoHoles[lastHole] = 0;
                    boardState.playerOneHoles[oppositeHole] = 0;
                    
                    yield return new WaitForSeconds(gameConfig.animationSpeed);
                    UpdateBoardVisuals();
                    UpdateUI();
                }
            }
        }
    }
    
    /// <summary>
    /// Check if the game has ended (one side has no seeds)
    /// </summary>
    private bool CheckGameEnd()
    {
        bool playerOneHasSeeds = false;
        bool playerTwoHasSeeds = false;
        
        for (int i = 0; i < 7; i++) // Changed to 7 holes
        {
            if (boardState.playerOneHoles[i] > 0) playerOneHasSeeds = true;
            if (boardState.playerTwoHoles[i] > 0) playerTwoHasSeeds = true;
        }
        
        return !playerOneHasSeeds || !playerTwoHasSeeds;
    }
    
    /// <summary>
    /// End the game and determine winner
    /// </summary>
    private void EndGame()
    {
        isGameActive = false;
        
        // Add remaining seeds to respective storehouses
        for (int i = 0; i < 7; i++) // Changed to 7 holes
        {
            boardState.playerOneScore += boardState.playerOneHoles[i];
            boardState.playerTwoScore += boardState.playerTwoHoles[i];
            boardState.playerOneHoles[i] = 0;
            boardState.playerTwoHoles[i] = 0;
        }
        
        // Determine winner
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
        
        Debug.Log($"Indonesian Congklak game ended! Winner: Player {boardState.winner}");
    }
    
    /// <summary>
    /// Get hole transform for animation
    /// </summary>
    private Transform GetHoleTransform(int holeIndex, bool isPlayerOneSide)
    {
        if (isPlayerOneSide)
        {
            return playerOneHoles[holeIndex];
        }
        else
        {
            return playerTwoHoles[holeIndex];
        }
    }
    
    /// <summary>
    /// Update visual representation of a specific hole
    /// </summary>
    private void UpdateHoleVisual(int holeIndex, bool isPlayerOneSide)
    {
        Transform holeTransform = GetHoleTransform(holeIndex, isPlayerOneSide);
        Hole holeComponent = holeTransform.GetComponent<Hole>();
        
        if (holeComponent != null)
        {
            int seedCount = isPlayerOneSide ? boardState.playerOneHoles[holeIndex] : boardState.playerTwoHoles[holeIndex];
            holeComponent.SetSeedCount(seedCount);
            
            // Update visual seeds if SeedPool exists
            if (SeedPool.Instance != null)
            {
                SeedPool.Instance.UpdateHoleSeeds(holeComponent);
            }
        }
    }
    
    /// <summary>
    /// Update visual representation of the board
    /// </summary>
    private void UpdateBoardVisuals()
    {
        // Update hole components for 7 holes
        for (int i = 0; i < 7; i++) // Changed to 7 holes
        {
            if (playerOneHoles[i] != null)
            {
                Hole holeComponent = playerOneHoles[i].GetComponent<Hole>();
                if (holeComponent != null)
                {
                    holeComponent.SetSeedCount(boardState.playerOneHoles[i]);
                    
                    // Update visual seeds if SeedPool exists
                    if (SeedPool.Instance != null)
                    {
                        SeedPool.Instance.UpdateHoleSeeds(holeComponent);
                    }
                }
            }
            
            if (playerTwoHoles[i] != null)
            {
                Hole holeComponent = playerTwoHoles[i].GetComponent<Hole>();
                if (holeComponent != null)
                {
                    holeComponent.SetSeedCount(boardState.playerTwoHoles[i]);
                    
                    // Update visual seeds if SeedPool exists
                    if (SeedPool.Instance != null)
                    {
                        SeedPool.Instance.UpdateHoleSeeds(holeComponent);
                    }
                }
            }
        }
        
        // Update storehouses
        if (playerOneStorehouse != null)
        {
            Hole storehouseComponent = playerOneStorehouse.GetComponent<Hole>();
            if (storehouseComponent != null)
            {
                storehouseComponent.SetSeedCount(boardState.playerOneScore);
                
                // Update visual seeds if SeedPool exists
                if (SeedPool.Instance != null)
                {
                    SeedPool.Instance.UpdateHoleSeeds(storehouseComponent);
                }
            }
        }
        
        if (playerTwoStorehouse != null)
        {
            Hole storehouseComponent = playerTwoStorehouse.GetComponent<Hole>();
            if (storehouseComponent != null)
            {
                storehouseComponent.SetSeedCount(boardState.playerTwoScore);
                
                // Update visual seeds if SeedPool exists
                if (SeedPool.Instance != null)
                {
                    SeedPool.Instance.UpdateHoleSeeds(storehouseComponent);
                }
            }
        }
    }
    
    /// <summary>
    /// Update UI elements
    /// </summary>
    private void UpdateUI()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateGameState(boardState);
        }
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        InitializeGame();
    }
}

[System.Serializable]
public class GameConfig
{
    [Header("Game Rules - Indonesian Congklak")]
    public int initialSeedsPerHole = 7;
    
    [Header("Animation")]
    public float animationSpeed = 0.3f;
    
    [Header("Audio")]
    public bool soundEnabled = true;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    
    [Header("AI")]
    public AILevel aiDifficulty = AILevel.Medium;
    public GameMode gameMode = GameMode.PlayerVsPlayer;
}

[System.Serializable]
public class BoardState
{
    public int[] playerOneHoles = new int[7]; // Changed to 7 holes
    public int[] playerTwoHoles = new int[7]; // Changed to 7 holes
    public int playerOneScore;
    public int playerTwoScore;
    public int currentPlayer;
    public GameStatus gameStatus;
    public int winner;
}

public enum GameStatus
{
    Playing,
    PlayerOneWins,
    PlayerTwoWins,
    Draw
}

public enum AILevel
{
    Easy,
    Medium,
    Hard
}

public enum GameMode
{
    PlayerVsPlayer,
    PlayerVsAI
}