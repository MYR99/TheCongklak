using UnityEngine;
using TMPro;

public enum HoleType { PlayerOne, PlayerTwo, Storehouse }

public class Hole : MonoBehaviour
{
    [SerializeField] private Transform seedContainer;
    [SerializeField] private TMP_Text seedText;
    [SerializeField] private HoleType holeType;
    [SerializeField] private int holeIndex;

    private int seedCount = 0;

    public HoleType GetHoleType() => holeType;
    public int GetHoleIndex() => holeIndex;

    public void SetHoleIndex(int index) => holeIndex = index;
    public void SetHoleType(HoleType type) => holeType = type;

    public void SetSeeds(int count)
    {
        seedCount = count;
        UpdateUI();
    }

    public int GetSeedCount() => seedCount;

    public void AddSeed()
    {
        seedCount++;
        UpdateUI();
    }

    public int TakeAllSeeds()
    {
        int taken = seedCount;
        seedCount = 0;
        UpdateUI();
        return taken;
    }

    private void UpdateUI()
    {
        if (seedText != null)
            seedText.text = seedCount.ToString();
    }
}