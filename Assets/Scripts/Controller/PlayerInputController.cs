using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private LayerMask holeLayerMask;
    [SerializeField] private Camera mainCamera;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleInput(Vector2 screenPos)
    {
        // ✅ block world click jika klik UI apapun
        if (IsPointerOverUIElement())
        {
            Debug.Log("[PlayerInputController] Click is on UI → block world click");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, holeLayerMask))
        {
            Hole hole = hit.collider.GetComponent<Hole>();
            if (hole != null)
            {
                Debug.Log($"[PlayerInputController] Clicked hole: {hole.name}, Index={hole.GetHoleIndex()}, Type={hole.GetHoleType()}");
                GameManager.Instance.SelectHole(hole);
            }
            else
            {
                Debug.Log("[PlayerInputController] Raycast hit but no Hole component");
            }
        }
        else
        {
            Debug.Log("[PlayerInputController] Raycast hit nothing!");
        }
    }

    // ===== Utility: cek pointer di UI =====
    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0; // ✅ asal kena UI apapun → true
    }
}