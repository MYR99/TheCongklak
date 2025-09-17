using UnityEngine;

[RequireComponent(typeof(Hole))]
public class HoleHighlight : MonoBehaviour
{
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
        if (hole == null || rend == null) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        bool canSelect = gm.isGameActive && gm.IsValidMove(GetHoleIndex());
        rend.material.color = canSelect ? highlightColor : originalColor;
    }

    private int GetHoleIndex()
    {
        return transform.GetSiblingIndex();
    }
}