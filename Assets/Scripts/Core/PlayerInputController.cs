using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [Header("Input Configuration")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask holeLayerMask = -1;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        if (gameCamera == null) gameCamera = Camera.main;
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

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        Vector2 screenPosition = inputActions.Player.Position.ReadValue<Vector2>();
        HandleInput(screenPosition);
    }

    private void HandleInput(Vector2 screenPosition)
    {
        Ray ray = gameCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, holeLayerMask))
        {
            Hole hole = hit.collider.GetComponent<Hole>();
            if (hole != null) ProcessHoleSelection(hole);
        }
    }

    private void ProcessHoleSelection(Hole hole)
    {
        // Only regular holes are selectable
        if (hole.GetHoleType() != HoleType.Regular) return;

        int holeIndex = hole.GetHoleIndex();

        // ✅ Let GameManager decide if it’s a valid move
        if (GameManager.Instance.IsValidMove(holeIndex))
        {
            GameManager.Instance.MakeMove(holeIndex);
        }
        else
        {
            Debug.Log("Invalid move: Hole is empty, protected, or not your turn.");
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        if (enabled) inputActions.Enable();
        else inputActions.Disable();
    }
}