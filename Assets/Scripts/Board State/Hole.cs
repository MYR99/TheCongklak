using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HoleType
{
    PlayerOne,
    PlayerTwo,
    Storehouse
}

public class Hole : MonoBehaviour
{
    [Header("Hole Settings")]
    [SerializeField] private HoleType holeType;
    [SerializeField] private int holeIndex;

    [Header("References")]
    [SerializeField] private Transform seedContainer;
    public TMP_Text seedText; // UI text untuk menampilkan jumlah biji

    private List<GameObject> seeds = new List<GameObject>();

    private void Awake()
    {
        if (seedContainer == null)
        {
            // kalau lupa assign, auto bikin container
            GameObject container = new GameObject("SeedContainer");
            container.transform.SetParent(this.transform);
            container.transform.localPosition = Vector3.zero;
            seedContainer = container.transform;
        }
    }

    // ========== HOLE CONFIG ==========

    public void SetHoleType(HoleType type) => holeType = type;
    public HoleType GetHoleType() => holeType;

    public void SetIndex(int index) => holeIndex = index;
    public int GetHoleIndex() => holeIndex;

    public Transform GetSeedContainer() => seedContainer;

    // ========== SEED MANAGEMENT ==========

    public void SetSeeds(int count)
    {
        ClearSeeds();
        for (int i = 0; i < count; i++)
        {
            GameObject seed = SeedPool.Instance.GetSeed();
            AddSeedObject(seed);
        }
        UpdateSeedText();
    }

    public void AddSeeds(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject seed = SeedPool.Instance.GetSeed();
            AddSeedObject(seed);
        }
        UpdateSeedText();
    }

    public void AddSeedObject(GameObject seed)
    {
        if (seed == null) return;
        seed.transform.SetParent(seedContainer);
        seed.transform.localPosition = GetRandomSeedPosition();
        seed.transform.localScale = Vector3.one * SeedPool.Instance.GetSeedScale();

        seeds.Add(seed);
        UpdateSeedText();
    }

    public List<GameObject> TakeAllSeedObjects()
    {
        List<GameObject> taken = new List<GameObject>(seeds);
        seeds.Clear();
        UpdateSeedText();
        return taken;
    }

    public int GetSeedCount() => seeds.Count;

    public int ClearSeeds()
    {
        int count = seeds.Count;
        foreach (var seed in seeds)
        {
            if (seed != null) SeedPool.Instance.ReturnSeed(seed);
        }
        seeds.Clear();
        UpdateSeedText();
        return count;
    }

    public Vector3 GetRandomSeedPosition()
    {
        return new Vector3(
            Random.Range(-0.1f, 0.1f),
            0f,
            Random.Range(-0.1f, 0.1f)
        );
    }

    // ========== UI UPDATE ==========

    private void UpdateSeedText()
    {
        if (seedText != null)
        {
            seedText.text = seeds.Count.ToString();
        }

        UIController ui = FindObjectOfType<UIController>();
        if (ui != null) ui.UpdateSeedCount(this, seeds.Count);
    }

    private bool UIControllerExists()
    {
        return FindObjectOfType<UIController>() != null;
    }
}