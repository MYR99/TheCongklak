using UnityEngine;

[System.Serializable]
public class BoardState
{
    // 7 holes per side
    public int[] playerOneHoles = new int[7];
    public int[] playerTwoHoles = new int[7];

    // Storehouses
    public int playerOneScore = 0;
    public int playerTwoScore = 0;

    // optional metadata
    public int currentPlayer = 1;
    public bool[] playerOneProtected = new bool[7];
    public bool[] playerTwoProtected = new bool[7];

    public GameStatus gameStatus = GameStatus.Playing;
    public int winner = 0;
}

public enum GameStatus { Playing, PlayerOneWins, PlayerTwoWins, Draw }