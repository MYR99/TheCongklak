using UnityEngine;

[RequireComponent(typeof(Hole))]
public class HoleHighlight : MonoBehaviour
{
    private Hole hole;
    private Renderer holeRenderer;
    private Color originalColor;
    public Color highlightColor = Color.yellow;

    private void Awake()
    {
        hole = GetComponent<Hole>();
        holeRenderer = GetComponent<Renderer>();
        if (holeRenderer != null)
        {
            originalColor = holeRenderer.material.color;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            // ✅ kirim langsung "hole" (bukan int)
            if (GameManager.Instance.IsValidMove(hole))
            {
                SetHighlight(true);
            }
            else
            {
                SetHighlight(false);
            }
        }
        else
        {
            SetHighlight(false);
        }
    }

    private void SetHighlight(bool state)
    {
        if (holeRenderer != null)
        {
            holeRenderer.material.color = state ? highlightColor : originalColor;
        }
    }
}