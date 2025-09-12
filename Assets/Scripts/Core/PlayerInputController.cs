using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [Header("Input Configuration")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask holeLayerMask = -1;
    
    [Header("Input Actions")]
    private PlayerInputActions inputActions;
    
    private void Awake()
    {
        // Initialize input actions
        inputActions = new PlayerInputActions();
        
        // Get main camera if not assigned
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Click.performed += OnClickPerformed;
    }
    
    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClickPerformed;
        inputActions.Disable();
    }

    /// <summary>
    /// Handle click/touch input
    /// </summary>
    private void OnClickPerformed(InputAction.CallbackContext context)
    {   
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return; Vector2 screenPosition = inputActions.Player.Position.ReadValue<Vector2>();
        HandleInput(screenPosition);
    }

    /// <summary>
    /// Process input at screen position
    /// </summary>
    private void HandleInput(Vector2 screenPosition)
    {
        Ray ray = gameCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, holeLayerMask))
        {
            Hole hole = hit.collider.GetComponent<Hole>();
            if (hole != null)
            {
                ProcessHoleSelection(hole);
            }
        }
    }
    
    /// <summary>
    /// Process hole selection
    /// </summary>
    private void ProcessHoleSelection(Hole hole)
    {
        // Only allow selection of regular holes that belong to current player
        if (hole.GetHoleType() != HoleType.Regular) return;
        if (!hole.BelongsToCurrentPlayer()) return;
        
        int holeIndex = hole.GetHoleIndex();
        
        if (GameManager.Instance.IsValidMove(holeIndex))
        {
            GameManager.Instance.MakeMove(holeIndex);
            
            // Provide audio feedback
            // AudioManager.Instance?.PlaySFX("hole_select");
        }
        else
        {
            // Provide feedback for invalid move
            // AudioManager.Instance?.PlaySFX("invalid_move");
            
            // Visual feedback could be added here
            Debug.Log("Invalid move: Hole is empty or not selectable");
        }
    }
    
    /// <summary>
    /// Enable/disable input processing
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        if (enabled)
        {
            inputActions.Enable();
        }
        else
        {
            inputActions.Disable();
        }
    }
}