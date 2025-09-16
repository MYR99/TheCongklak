using UnityEngine;
using TMPro;

public class Hole : MonoBehaviour
{
    [Header("Hole Settings")]
    [SerializeField] private HoleType holeType = HoleType.Regular;
    [SerializeField] private int holeIndex = 0; // Always 0–6 for Player or Opponent
    [SerializeField] private Transform seedContainer;
    [SerializeField] private MeshRenderer holeRenderer;
    [SerializeField] private TextMeshProUGUI seedText; // Optional TMP overlay

    private int seedCount;
    private bool isProtected = false;

    private Color normalColor = Color.white;
    private Color protectedColor = Color.gray;

    private void Awake()
    {
        if (holeRenderer == null)
            holeRenderer = GetComponent<MeshRenderer>();
    }

    // --- Seed Management ---
    public void SetSeedCount(int count)
    {
        seedCount = count;
        if (seedText != null)
            seedText.text = seedCount.ToString();
    }

    public int GetSeedCount() => seedCount;

    // --- Hole Metadata ---
    public int GetHoleIndex() => holeIndex;
    public HoleType GetHoleType() => holeType;
    public Transform GetSeedContainer() => seedContainer;

    // --- Player Ownership ---
    public bool BelongsToCurrentPlayer()
    {
        if (GameManager.Instance == null) return false;
        if (holeType != HoleType.Regular) return false;

        int currentPlayer = GameManager.Instance.currentPlayer;

        // ✅ We only check turn here
        // GameManager.IsValidMove(holeIndex) already verifies seeds + protection
        if (currentPlayer == 1) return true;
        if (currentPlayer == 2) return true;

        return false;
    }

    // --- Protected Hole (Ngacang) ---
    public void SetProtected(bool state)
    {
        isProtected = state;

        if (holeRenderer != null)
            holeRenderer.material.color = state ? protectedColor : normalColor;

        if (seedText != null)
            seedText.color = state ? protectedColor : normalColor;
    }

    public bool IsProtected() => isProtected;
}

public enum HoleType
{
    Regular,
    Storehouse
}