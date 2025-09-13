using UnityEngine;

public class BoardSeedInitializer : MonoBehaviour
{
    [SerializeField] private Hole[] allHoles; // Assign all player and opponent holes in the Inspector
    [SerializeField] private int seedsPerHole = 7; // Or get from GameConfig

    private void Start()
    {
        InitializeAllHoles();
    }

    private void InitializeAllHoles()
    {
        foreach (Hole hole in allHoles)
        {
            if (hole.GetHoleType() == HoleType.Regular)
            {
                hole.SetSeedCount(seedsPerHole);
                if (SeedPool.Instance != null)
                {
                    SeedPool.Instance.UpdateHoleSeeds(hole);
                }
            }
        }
    }
}
