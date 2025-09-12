using UnityEngine;
using TMPro;

public class Hole : MonoBehaviour
{
    [Header("Hole Configuration")]
    [SerializeField] private int holeIndex;
    [SerializeField] private HoleType holeType;
    [SerializeField] private int playerOwner; // 1 or 2
    
    [Header("Visual Components")]
    [SerializeField] private TextMeshProUGUI seedCountText;
    [SerializeField] private Transform seedContainer;
    
    [Header("Current State")]
    [SerializeField] private int currentSeedCount;
    
    private void Start()
    {
        // Auto-determine hole index from name if not set
        if (holeIndex == -1)
        {
            DetermineHoleIndex();
        }
        
        // Initialize visual components
        if (seedCountText == null)
        {
            seedCountText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (seedContainer == null)
        {
            seedContainer = transform.Find("SeedContainer");
            if (seedContainer == null)
            {
                // Create seed container if it doesn't exist
                GameObject container = new GameObject("SeedContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                seedContainer = container.transform;
            }
        }
    }
    
    /// <summary>
    /// Set the seed count for this hole
    /// </summary>
    public void SetSeedCount(int count)
    {
        currentSeedCount = count;
        UpdateVisualDisplay();
    }
    
    /// <summary>
    /// Get the current seed count
    /// </summary>
    public int GetSeedCount()
    {
        return currentSeedCount;
    }
    
    /// <summary>
    /// Get the hole index
    /// </summary>
    public int GetHoleIndex()
    {
        return holeIndex;
    }
    
    /// <summary>
    /// Get the hole type
    /// </summary>
    public HoleType GetHoleType()
    {
        return holeType;
    }
    
    /// <summary>
    /// Get the player owner
    /// </summary>
    public int GetPlayerOwner()
    {
        return playerOwner;
    }
    
    /// <summary>
    /// Check if this hole belongs to the current player
    /// </summary>
    public bool BelongsToCurrentPlayer()
    {
        return playerOwner == GameManager.Instance.currentPlayer;
    }
    
    /// <summary>
    /// Handle mouse click on this hole
    /// </summary>
    private void OnMouseDown()
    {
        if (GameManager.Instance != null && holeType == HoleType.Regular)
        {
            if (BelongsToCurrentPlayer() && GameManager.Instance.IsValidMove(holeIndex))
            {
                GameManager.Instance.MakeMove(holeIndex);
            }
        }
    }
    
    /// <summary>
    /// Update the visual display of seed count
    /// </summary>
    private void UpdateVisualDisplay()
    {
        if (seedCountText != null)
        {
            seedCountText.text = currentSeedCount.ToString();
        }
        // Additional visual updates will be handled by SeedPool in Phase 4
    }
    
    /// <summary>
    /// Automatically determine hole index from GameObject name
    /// </summary>
    private void DetermineHoleIndex()
    {
        string objectName = gameObject.name.ToLower();
        
        // Try to extract number from name
        for (int i = 0; i < objectName.Length; i++)
        {
            if (char.IsDigit(objectName[i]))
            {
                if (int.TryParse(objectName[i].ToString(), out int index))
                {
                    holeIndex = index;
                    return;
                }
            }
        }
        
        // Fallback: use transform sibling index
        holeIndex = transform.GetSiblingIndex();
    }
    
    /// <summary>
    /// Get the seed container transform for visual seed placement
    /// </summary>
    public Transform GetSeedContainer()
    {
        return seedContainer;
    }
}

public enum HoleType
{
    Regular,
    Storehouse
}