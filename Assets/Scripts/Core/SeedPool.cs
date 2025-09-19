using System.Collections.Generic;
using UnityEngine;

public class SeedPool : MonoBehaviour
{
    public static SeedPool Instance;

    [Header("Seed Pool Settings")]
    [SerializeField] private GameObject seedPrefab;   // prefab biji
    [SerializeField] private int initialPoolSize = 100; 

    private Queue<GameObject> availableSeeds = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // fallback kalau prefab belum diassign → buat sphere default
        if (seedPrefab == null)
        {
            Debug.LogWarning("⚠️ SeedPrefab belum diassign, membuat sphere default.");
            seedPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var collider = seedPrefab.GetComponent<Collider>();
            if (collider != null) Destroy(collider); // hapus collider supaya tidak ganggu raycast
        }

        FillPool(initialPoolSize);
    }

    private void FillPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject seed = Instantiate(seedPrefab, transform);
            seed.SetActive(false);
            availableSeeds.Enqueue(seed);
        }
    }

    public GameObject GetSeed()
    {
        if (availableSeeds.Count == 0)
        {
            FillPool(10);
        }

        GameObject seed = availableSeeds.Dequeue();

        if (seed == null)
        {
            Debug.LogWarning("⚠️ Seed prefab null, membuat Sphere default.");
            seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        // aktifkan & set ukuran 500f
        seed.SetActive(true);
        seed.transform.localScale = Vector3.one * 500f;

        Debug.Log($"🌱 Spawn seed {seed.name} dengan scale {seed.transform.localScale}");

        return seed;
    }

    public void ReturnSeed(GameObject seed)
    {
        seed.SetActive(false);
        seed.transform.SetParent(transform);
        availableSeeds.Enqueue(seed);
    }
}