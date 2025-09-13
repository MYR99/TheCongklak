using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPool : MonoBehaviour
{
    [Header("Seed Visual Settings")]
        [SerializeField] private float seedScale = 800f; // Adjust this in Inspector for seed size
    public static SeedPool Instance;

    [Header("Pool Configuration")]
    [SerializeField] private GameObject seedPrefab;
    [SerializeField] private int initialPoolSize = 100;
    [Header("Seed Placement Settings")]
    [Tooltip("Offset applied to each seed's local position inside the hole. Adjust in Inspector for perfect placement.")]
    public Vector3 seedOffset = Vector3.zero;
    [SerializeField] private Transform poolParent;

    [Header("Animation Settings")]
    [SerializeField] private float seedMoveSpeed = 5f;
    [SerializeField] private float seedMoveHeight = 1f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Queue<GameObject> seedPool = new Queue<GameObject>();
    private List<GameObject> activeSeeds = new List<GameObject>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePool();
    }

    /// <summary>
    /// Initialize the seed pool with pre-instantiated objects
    /// </summary>
    private void InitializePool()
    {
        // Create seed prefab if not assigned
        if (seedPrefab == null)
        {
            Debug.LogWarning("SeedPool: Creating default seed prefab!");
            CreateDefaultSeedPrefab();
        }

        // Create pool parent if not assigned
        if (poolParent == null)
        {
            GameObject poolParentObj = new GameObject("SeedPool");
            poolParentObj.transform.SetParent(transform);
            poolParent = poolParentObj.transform;
        }

        // Pre-instantiate seeds
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject seed = Instantiate(seedPrefab, poolParent);
            seed.SetActive(false);
            seedPool.Enqueue(seed);
        }

        Debug.Log($"SeedPool initialized with {initialPoolSize} seeds");
    }

    /// <summary>
    /// Create a default seed prefab if none is assigned
    /// </summary>
    private void CreateDefaultSeedPrefab()
    {
        GameObject seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        seed.name = "DefaultSeed";
        seed.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // Create brown material
        Material seedMaterial = new Material(Shader.Find("Standard"));
        seedMaterial.color = new Color(0.6f, 0.4f, 0.2f);
        seed.GetComponent<Renderer>().material = seedMaterial;

        seedPrefab = seed;

        Debug.Log("Created default seed prefab");
    }

    /// <summary>
    /// Get a seed from the pool
    /// </summary>
    public GameObject GetSeed()
    {
        GameObject seed;

        if (seedPool.Count > 0)
        {
            seed = seedPool.Dequeue();
        }
        else
        {
            // Create new seed if pool is empty
            seed = Instantiate(seedPrefab, poolParent);
            Debug.Log("SeedPool: Created additional seed - consider increasing pool size");
        }

        seed.SetActive(true);
        activeSeeds.Add(seed);
        return seed;
    }

    /// <summary>
    /// Return a seed to the pool
    /// </summary>
    public void ReturnSeed(GameObject seed)
    {
        if (seed == null) return;

        seed.SetActive(false);
        seed.transform.SetParent(poolParent);
        seed.transform.localPosition = Vector3.zero;
        seed.transform.localRotation = Quaternion.identity;

        activeSeeds.Remove(seed);
        seedPool.Enqueue(seed);
    }

    /// <summary>
    /// Return multiple seeds to the pool
    /// </summary>
    public void ReturnSeeds(List<GameObject> seeds)
    {
        foreach (GameObject seed in seeds)
        {
            ReturnSeed(seed);
        }
    }

    /// <summary>
    /// Return all active seeds to the pool
    /// </summary>
    public void ReturnAllSeeds()
    {
        List<GameObject> seedsToReturn = new List<GameObject>(activeSeeds);
        foreach (GameObject seed in seedsToReturn)
        {
            ReturnSeed(seed);
        }
    }

    /// <summary>
    /// Create visual seeds for a hole
    /// </summary>
    public List<GameObject> CreateSeedsForHole(Transform holeTransform, int seedCount)
    {
        List<GameObject> seeds = new List<GameObject>();

        // Get or create seed container
        Transform seedContainer = holeTransform.Find("SeedContainer");
        if (seedContainer == null)
        {
            GameObject container = new GameObject("SeedContainer");
            container.transform.SetParent(holeTransform);
            container.transform.localPosition = Vector3.zero;
            seedContainer = container.transform;
        }

        for (int i = 0; i < seedCount; i++)
        {
            GameObject seed = GetSeed();
            seed.transform.SetParent(seedContainer);

            // Position seeds in a circular pattern
            float angle = (360f / Mathf.Max(1, seedCount)) * i * Mathf.Deg2Rad;
            float radius = 0.01f; // Much smaller radius for tight cluster

            Vector3 localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                0.5f, // Slightly above the bottom of the hole
                Mathf.Sin(angle) * radius
            );

            seed.transform.localPosition = localPosition;
            seed.transform.localScale = Vector3.one * 50f;
            seeds.Add(seed);
        }

        Debug.Log($"Created {seedCount} seeds for hole {holeTransform.name}");
        return seeds;
    }

    /// <summary>
    /// Animate seed movement from one hole to another
    /// </summary>
    public IEnumerator AnimateSeedMovement(GameObject seed, Transform fromHole, Transform toHole, float delay = 0f)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Vector3 startPos = fromHole.position;
        Vector3 endPos = toHole.position;
        Vector3 midPoint = (startPos + endPos) / 2f + Vector3.up * seedMoveHeight;

        float elapsedTime = 0f;
        float duration = 1f / seedMoveSpeed;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = movementCurve.Evaluate(t);

            // Bezier curve movement for arc effect
            Vector3 currentPos = CalculateBezierPoint(startPos, midPoint, endPos, curveValue);
            seed.transform.position = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        seed.transform.position = endPos;
        seed.transform.SetParent(toHole);

        // Add some rotation for visual effect
        seed.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
    }

    /// <summary>
    /// Calculate point on bezier curve
    /// </summary>
    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;

        return point;
    }

    /// <summary>
    /// Update visual representation of seeds in a hole
    /// </summary>
    public void UpdateHoleSeeds(Hole hole)
    {
        if (hole == null) return;

        Transform seedContainer = hole.GetSeedContainer();
        int targetSeedCount = hole.GetSeedCount();

        // Remove all existing seeds from this container (return to pool)
        List<GameObject> existingSeeds = new List<GameObject>();
        for (int i = seedContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = seedContainer.GetChild(i).gameObject;
            if (child.CompareTag("Seed") || child.name.Contains("Seed") || child.name.Contains("DefaultSeed") || child.name.Contains("Walnut"))
            {
                existingSeeds.Add(child);
            }
        }
        
        ReturnSeeds(existingSeeds);

        // Create and parent new seeds to the correct SeedContainer, spread in a circle
        float radius = 0.3f; // Adjust for your board size
        for (int i = 0; i < targetSeedCount; i++)
        {
            GameObject seed = GetSeed();
            seed.transform.SetParent(seedContainer);
            // Spread seeds in a circle
            float angle = (360f / Mathf.Max(1, targetSeedCount)) * i * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                0f, // Y offset, keep at 0 unless you want to lift seeds
                Mathf.Sin(angle) * radius
            );
            seed.transform.localPosition = localPosition;
            seed.transform.localRotation = Quaternion.identity;
            seed.transform.localScale = Vector3.one * seedScale; // Use configurable scale
        }
    }

    /// <summary>
    /// Force update all holes with seeds
    /// </summary>
    [ContextMenu("Force Update All Holes")]
    public void ForceUpdateAllHoles()
    {
        Hole[] allHoles = FindObjectsOfType<Hole>();
        foreach (Hole hole in allHoles)
        {
            if (hole.GetSeedCount() > 0)
            {
                UpdateHoleSeeds(hole);
            }
        }
        Debug.Log($"Force updated {allHoles.Length} holes with seeds");
    }

    /// <summary>
    /// Get current pool statistics
    /// </summary>
    public void LogPoolStats()
    {
        Debug.Log($"SeedPool Stats - Available: {seedPool.Count}, Active: {activeSeeds.Count}");
    }
}