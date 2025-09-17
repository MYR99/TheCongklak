using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Hole hole = hit.collider.GetComponent<Hole>();
                if (hole != null)
                {
                    // ambil seeds
                    int seeds = hole.TakeAllSeeds();
                    if (seeds > 0)
                    {
                        Debug.Log($"Picked hole {hole.GetHoleType()} index {hole.GetHoleIndex()} with {seeds} seeds");

                        // TODO: di sini jalankan sowing/distribution pakai GameManager
                        // sementara panggil OnTurnFinished
                        GameManager.Instance.OnTurnFinished();
                    }
                }
            }
        }
    }
}