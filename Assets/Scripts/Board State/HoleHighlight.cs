using UnityEngine;

public class HoleHighlight : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material validMoveMaterial;
    [SerializeField] private Material invalidMoveMaterial;
    
    [Header("Components")]
    private Renderer holeRenderer;
    private Hole holeComponent;
    
    [Header("State")]
    private bool isHighlighted = false;
    private bool isValidMove = false;
    
    private void Start()
    {
        // Get required components
        holeRenderer = GetComponent<Renderer>();
        holeComponent = GetComponent<Hole>();
        
        // Set initial material
        if (holeRenderer != null && normalMaterial != null)
        {
            holeRenderer.material = normalMaterial;
        }
    }
    
    private void Update()
    {
        // Update highlight state based on game state
        UpdateHighlightState();
    }

    /// <summary>
    /// Update the highlight state based on current game conditions
    /// </summary>
    private void UpdateHighlightState()
    {
        if (holeComponent == null || GameManager.Instance == null) return;
        bool wasValidMove = isValidMove;
        isValidMove = CanPlayerSelectHole();

        if (wasValidMove != isValidMove)
        {
            UpdateMaterial();
        }
    }

    /// <summary>
    /// Check if the current player can select this hole
    /// </summary>
    private bool CanPlayerSelectHole()
    {
        if (holeComponent == null || GameManager.Instance == null) return false;
        if (holeComponent.GetHoleType() != HoleType.Regular) return false;
        if (!GameManager.Instance.isGameActive) return false;
        if (!holeComponent.BelongsToCurrentPlayer()) return false;
        return GameManager.Instance.IsValidMove(holeComponent.GetHoleIndex());
    }

    /// <summary>
    /// Handle mouse enter event
    /// </summary>
    private void OnMouseEnter()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return; isHighlighted = true;
        UpdateMaterial();

        if (isValidMove)
        {
            // AudioManager.Instance?.PlaySFX("hole_hover");    
        }
    }

    /// <summary>
    /// Handle mouse exit event
    /// </summary>
    private void OnMouseExit()
    {
        isHighlighted = false;
        UpdateMaterial();
    }
    
    /// <summary>
    /// Update the material based on current state
    /// </summary>
    private void UpdateMaterial()
    {
        if (holeRenderer == null) return;
        
        Material targetMaterial = normalMaterial;
        
        if (isHighlighted)
        {
            if (isValidMove)
            {
                targetMaterial = validMoveMaterial != null ? validMoveMaterial : highlightMaterial;
            }
            else
            {
                targetMaterial = invalidMoveMaterial != null ? invalidMoveMaterial : highlightMaterial;
            }
        }
        else if (isValidMove && !isHighlighted)
        {
            // Subtle indication for valid moves even when not hovered
            targetMaterial = normalMaterial;
        }
        
        if (targetMaterial != null)
        {
            holeRenderer.material = targetMaterial;
        }
    }
    
    /// <summary>
    /// Force update the highlight state (useful for external calls)
    /// </summary>
    public void ForceUpdateHighlight()
    {
        UpdateHighlightState();
        UpdateMaterial();
    }
    
    /// <summary>
    /// Set custom highlight state
    /// </summary>
    public void SetHighlightState(bool highlighted, bool valid)
    {
        isHighlighted = highlighted;
        isValidMove = valid;
        UpdateMaterial();
    }
}