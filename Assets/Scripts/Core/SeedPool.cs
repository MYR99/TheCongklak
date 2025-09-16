using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPool : MonoBehaviour
{
    public static SeedPool Instance;

    [Header("Seed Settings")]
    [SerializeField] private GameObject seedPrefab;
    [SerializeField] private int initialPoolSize = 100;
    [SerializeField] private float seedScale = 0.05f;
    [SerializeField] private float animationDuration = 0.25f;

    private Queue<GameObject> seedPool = new Queue<GameObject>();
    private List<GameObject> activeSeeds = new List<GameObject>();
    private Transform poolParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolParent = new GameObject("SeedPoolParent").transform;
        poolParent.SetParent(transform);

        if (seedPrefab == null)
            CreateDefaultSeedPrefab();

        InitializePool();
    }

    private void CreateDefaultSeedPrefab()
    {
        GameObject seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        seed.name = "DefaultSeed";
        seed.transform.localScale = Vector3.one * seedScale;
        seed.tag = "Seed";

        Material seedMaterial = new Material(Shader.Find("Standard"));
        seedMaterial.color = new Color(0.6f, 0.4f, 0.2f);
        seed.GetComponent<Renderer>().material = seedMaterial;

        seedPrefab = seed;
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject seed = Instantiate(seedPrefab, poolParent);
            seed.SetActive(false);
            seed.tag = "Seed";
            seedPool.Enqueue(seed);
        }
    }

    public GameObject GetSeed()
    {
        GameObject seed;
        if (seedPool.Count > 0) seed = seedPool.Dequeue();
        else seed = Instantiate(seedPrefab, poolParent);

        seed.SetActive(true);
        seed.tag = "Seed";
        seed.transform.localScale = Vector3.one * Mathf.Max(0.01f, seedScale);
        activeSeeds.Add(seed);
        return seed;
    }

    public void ReturnSeed(GameObject seed)
    {
        if (seed == null) return;
        seed.SetActive(false);
        seed.transform.SetParent(poolParent);
        activeSeeds.Remove(seed);
        seedPool.Enqueue(seed);
    }

    public void ReturnSeeds(List<GameObject> seeds)
    {
        foreach (var seed in seeds) ReturnSeed(seed);
    }

    public void UpdateHoleSeeds(Hole hole)
    {
        if (hole == null) return;
        Transform container = hole.GetSeedContainer();
        if (container == null) return;

        // Clear old seeds
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject seed = container.GetChild(i).gameObject;
            if (seed.CompareTag("Seed")) ReturnSeed(seed);
        }

        int targetSeedCount = hole.GetSeedCount();
        float radius = 0.2f;

        for (int i = 0; i < targetSeedCount; i++)
        {
            GameObject seed = GetSeed();
            seed.transform.SetParent(container);

            float angle = (360f / Mathf.Max(1, targetSeedCount)) * i * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            seed.transform.localPosition = localPosition;
            seed.transform.localRotation = Quaternion.identity;
            seed.transform.localScale = Vector3.one * Mathf.Max(0.01f, seedScale);
        }
    }

    public IEnumerator AnimateSeedMovement(GameObject seed, Transform from, Transform to, float delay)
    {
        if (seed == null || from == null || to == null) yield break;

        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        Vector3 startPos = from.position;
        Vector3 targetPos = to.position;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            seed.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        seed.transform.position = targetPos;
        seed.transform.SetParent(to);
    }
}