using System.Collections;
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
    public TMP_Text seedText;
    public GameObject turnIndicatorHighlight;

    private List<GameObject> seeds = new List<GameObject>();

    private void Awake()
    {
        if (seedContainer == null)
        {
            GameObject container = new GameObject("SeedContainer");
            container.transform.SetParent(this.transform);
            container.transform.localPosition = Vector3.zero;
            seedContainer = container.transform;
        }
    }

    // ✅ Tambahkan deteksi klik (gunakan collider 3D, non-trigger)
    private void OnMouseDown()
    {
        // Abaikan klik jika sedang ada UI di atas atau game pause
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        // Kirim klik ke GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log($"[Hole] Clicked on {name}");
            GameManager.Instance.SelectHole(this);
        }
    }

    public void SetHoleType(HoleType type) => holeType = type;
    public HoleType GetHoleType() => holeType;
    public void SetIndex(int index) => holeIndex = index;
    public int GetHoleIndex() => holeIndex;
    public Transform GetSeedContainer() => seedContainer;

    public void SetSeeds(int count)
    {
        ClearSeeds();
        for (int i = 0; i < count; i++)
        {
            AddSeedObject(SeedPool.Instance.GetSeed());
        }
    }

    public void AddSeeds(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddSeedObject(SeedPool.Instance.GetSeed());
        }
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

    public IEnumerator AnimateAddSeedCoroutine(GameObject seed, Transform startContainer, float liftHeight, float moveSpeed)
    {
        seed.transform.SetParent(this.seedContainer, true);
        Vector3 startPos = startContainer.position;
        Vector3 endPos = this.seedContainer.position;
        Vector3 finalLocalPos = GetRandomSeedPosition();
        Vector3 startLiftPos = startPos + Vector3.up * liftHeight;
        Vector3 endLiftPos = endPos + Vector3.up * liftHeight;
        Vector3 finalWorldPos = this.seedContainer.TransformPoint(finalLocalPos);

        while (Vector3.Distance(seed.transform.position, startLiftPos) > 0.01f)
        {
            seed.transform.position = Vector3.MoveTowards(seed.transform.position, startLiftPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        while (Vector3.Distance(seed.transform.position, endLiftPos) > 0.01f)
        {
            seed.transform.position = Vector3.MoveTowards(seed.transform.position, endLiftPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        while (Vector3.Distance(seed.transform.position, finalWorldPos) > 0.01f)
        {
            seed.transform.position = Vector3.MoveTowards(seed.transform.position, finalWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        seed.transform.localPosition = finalLocalPos;
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

    private void UpdateSeedText()
    {
        if (seedText != null)
        {
            seedText.text = seeds.Count.ToString();
        }
    }

    public void SetTurnHighlight(bool state)
    {
        if (turnIndicatorHighlight != null)
        {
            turnIndicatorHighlight.SetActive(state);
        }
    }
}