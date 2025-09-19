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
                    // --- Cek apakah hole milik player yang sedang jalan ---
                    bool isPlayerOneTurn = GameManager.Instance.CurrentPlayer == 1;
                    bool isValid =
                        (isPlayerOneTurn && hole.GetHoleType() == HoleType.PlayerOne) ||
                        (!isPlayerOneTurn && hole.GetHoleType() == HoleType.PlayerTwo);

                    if (!isValid)
                    {
                        Debug.Log("❌ Tidak boleh pilih lubang lawan!");
                        return;
                    }

                    // --- Ambil seeds kalau valid ---
                    int seeds = hole.TakeAllSeeds();
                    if (seeds > 0)
                    {
                        Debug.Log($"✅ Picked hole {hole.GetHoleType()} index {hole.GetHoleIndex()} with {seeds} seeds");
                        
                        // Mulai proses sowing (penyebaran biji)
                        GameManager.Instance.StartCoroutine(
                            GameManager.Instance.SowSeeds(hole, seeds)
                        );
                    }
                }
            }
        }
    }
}