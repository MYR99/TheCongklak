using UnityEngine;

[RequireComponent(typeof(Hole))]
public class HoleHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;

    private Hole hole;
    private Renderer rend;
    private Color originalColor;

    private void Awake()
    {
        hole = GetComponent<Hole>();
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    private void Update()
    {
        UpdateHighlightState();
    }

    private void UpdateHighlightState()
    {
        if (hole == null || rend == null) return;

        bool canSelect = false;
        var gm = GameManager.Instance;

        // Highlight only when the game is active and GameManager says it's a valid move.
        // GameManager.IsValidMove(...) already checks: current player, seed count and protected state.
        if (gm != null && gm.isGameActive)
        {
            int holeIndex = hole.GetHoleIndex();
            canSelect = gm.IsValidMove(holeIndex);
        }

        // Set color (using material instance). If many holes, consider using a shared shader property instead.
        rend.material.color = canSelect ? highlightColor : originalColor;
    }
}