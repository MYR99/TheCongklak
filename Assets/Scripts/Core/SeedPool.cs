using UnityEngine;
using System.Collections;

public class SeedPool : MonoBehaviour
{
    [SerializeField] private GameObject seedPrefab;

    public IEnumerator DistributeSeeds(Hole[] holes, int startIndex, int seeds)
    {
        int index = startIndex;

        while (seeds > 0)
        {
            holes[index].AddSeed();
            seeds--;

            yield return new WaitForSeconds(0.2f); // delay animasi

            index = (index + 1) % holes.Length;
        }
    }
}