using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Congklak/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Gameplay Settings")]
    public int initialSeedsPerHole = 7;
    public float animationSpeed = 0.1f;

    [Header("Audio Settings")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public bool soundEnabled = true;

    [Header("AI Settings")]
    public AILevel aiDifficulty = AILevel.Medium;
    public GameMode gameMode = GameMode.PlayerVsPlayer;
}