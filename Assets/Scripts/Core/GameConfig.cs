using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Congklak/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Gameplay Settings")]
    [Tooltip("Initial seeds per hole (typical Congklak = 7)")]
    public int initialSeedsPerHole = 7;

    [Tooltip("Delay between each seeded drop when animating (seconds)")]
    public float animationSpeed = 0.1f;

    [Header("Audio Settings")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public bool soundEnabled = true;

    [Header("AI & Mode")]
    public AILevel aiDifficulty = AILevel.Medium;
    public GameMode gameMode = GameMode.PlayerVsPlayer;
}

public enum AILevel { Easy, Medium, Hard }
public enum GameMode { PlayerVsPlayer, PlayerVsAI }