using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HoleType { PlayerOne, PlayerTwo, Storehouse }
public enum SpawnMode { Circle, Random }

[RequireComponent(typeof(Transform))]
public class Hole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform seedContainer;
    [SerializeField] private TMP_Text seedText;

    [Header("Identity")]
    [SerializeField] private HoleType holeType = HoleType.PlayerOne;
    [SerializeField] private int holeIndex = 0;

    [Header("Spawn Settings")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Circle;
    [Tooltip("Uniform scale untuk seeds.")]
    [SerializeField] private float seedScale = 500f;
    [Range(0.1f, 1f)]
    [Tooltip("Seberapa rapat seeds terkumpul di tengah hole (0.1 = rapat, 1.0 = longgar).")]
    [SerializeField] private float spawnTightness = 0.3f;

    private float spawnRadius = 1f; 
    private float seedHeight = 0f; 
    private List<GameObject> seedObjects = new List<GameObject>();
    private int seedCount = 0;

    private void Awake()
    {
        // Hitung radius & tinggi otomatis
        if (TryGetComponent(out SphereCollider sphereCol))
        {
            spawnRadius = sphereCol.radius * transform.localScale.x * spawnTightness;
            seedHeight = (sphereCol.center.y - sphereCol.radius) * transform.localScale.y + 0.05f;
        }
        else if (TryGetComponent(out MeshRenderer mesh))
        {
            float size = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.z);
            spawnRadius = size * 0.5f * spawnTightness;
            seedHeight = mesh.bounds.min.y + 0.05f;
        }
        else
        {
            Debug.LogWarning($"⚠️ Hole {name} tidak punya collider/mesh, pakai default radius 1f & height 0f");
            spawnRadius = 1f * spawnTightness;
            seedHeight = 0f;
        }
    }

    // ----- Public API -----
    public HoleType GetHoleType() => holeType;
    public int GetHoleIndex() => holeIndex;
    public void SetHoleIndex(int i) => holeIndex = i;
    public void SetHoleType(HoleType t) => holeType = t;

    public void SetSeeds(int count)
    {
        if (count < 0) count = 0;
        ClearSeedObjects();
        seedCount = count;
        SpawnAllSeeds();
        UpdateUI();
    }

    public int GetSeedCount() => seedCount;

    public void AddSeed()
    {
        seedCount++;
        ClearSeedObjects();
        SpawnAllSeeds();
        UpdateUI();
    }

    public int TakeAllSeeds()
    {
        int taken = seedCount;
        seedCount = 0;
        ClearSeedObjects();
        UpdateUI();
        return taken;
    }

    // ----- Spawning -----
    private void SpawnAllSeeds()
    {
        if (seedCount <= 0) return;
        if (seedContainer == null)
        {
            Debug.LogWarning($"⚠️ Hole '{name}' tidak ada seedContainer.");
            return;
        }

        if (SeedPool.Instance == null)
        {
            Debug.LogWarning("⚠️ SeedPool.Instance null — tidak bisa spawn seeds.");
            return;
        }

        if (spawnMode == SpawnMode.Circle)
        {
            for (int i = 0; i < seedCount; i++)
                SpawnSeedCircular(i, seedCount);
        }
        else
        {
            for (int i = 0; i < seedCount; i++)
                SpawnSeedRandom();
        }
    }

    private void SpawnSeedCircular(int index, int total)
    {
        GameObject seed = GetSeedFromPoolFallback();
        seed.transform.SetParent(seedContainer, false);

        float angle = (360f / Mathf.Max(1, total)) * index;
        float rad = angle * Mathf.Deg2Rad;
        float r = spawnRadius;

        float x = Mathf.Cos(rad) * r;
        float z = Mathf.Sin(rad) * r;

        seed.transform.localPosition = new Vector3(x, seedHeight, z);
        seed.transform.localRotation = Quaternion.identity;
        seed.transform.localScale = Vector3.one * seedScale;

        seedObjects.Add(seed);
    }

    private void SpawnSeedRandom()
    {
        GameObject seed = GetSeedFromPoolFallback();
        seed.transform.SetParent(seedContainer, false);

        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        seed.transform.localPosition = new Vector3(circle.x, seedHeight, circle.y);
        seed.transform.localRotation = Quaternion.identity;
        seed.transform.localScale = Vector3.one * seedScale;

        seedObjects.Add(seed);
    }

    private GameObject GetSeedFromPoolFallback()
    {
        GameObject seed = null;
        if (SeedPool.Instance != null)
            seed = SeedPool.Instance.GetSeed();

        if (seed == null)
        {
            seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var col = seed.GetComponent<Collider>();
            if (col) Destroy(col);
        }
        return seed;
    }

    private void ClearSeedObjects()
    {
        if (seedObjects == null || seedObjects.Count == 0) return;

        foreach (var s in seedObjects)
        {
            if (s == null) continue;
            if (SeedPool.Instance != null) SeedPool.Instance.ReturnSeed(s);
            else Destroy(s);
        }
        seedObjects.Clear();
    }

    private void UpdateUI()
    {
        if (seedText != null)
            seedText.text = seedCount.ToString();
    }

    // Debug gizmo
    private void OnDrawGizmosSelected()
    {
        if (seedContainer == null) return;
        Gizmos.color = Color.cyan;

        Vector3 center = seedContainer.position;
        const int segments = 32;
        Vector3 prev = center + new Vector3(spawnRadius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(t) * spawnRadius, 0f, Mathf.Sin(t) * spawnRadius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}