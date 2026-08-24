using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class Silo : MonoBehaviour, ICardDropProcessor
{
    [SerializeField] private int baseStorageCapacity = 1;
    [SerializeField] private int upgradeCapacityIncrease = 1;
    [SerializeField] private int baseUpgradeCost = 10;
    [SerializeField] private int upgradeCostMultiplier = 20;
    [SerializeField] private GameObject storedWaterBucketPrefab;
    [SerializeField] private TMP_Text storageCountText;
    [SerializeField] private TMP_Text upgradePriceText;

    private int currentStoredBuckets;
    private int upgradeCount;
    private int storageCapacity;
    private List<GameObject> spawnedBuckets = new List<GameObject>();
    private BoxCollider2D siloCollider;

    private void Awake()
    {
        siloCollider = GetComponent<BoxCollider2D>();
        upgradeCount = 0;
        storageCapacity = baseStorageCapacity;
        currentStoredBuckets = 0;
        UpdateStorageDisplay();
        UpdateUpgradePriceDisplay();
    }

    public bool TryHandleDrop(Vector3 worldPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in colliders)
        {
            if (hit == siloCollider)
                continue;

            BucketCard bucketCard = hit.GetComponentInParent<BucketCard>();
            if (bucketCard == null)
                continue;

            if (currentStoredBuckets >= storageCapacity)
                return false;

            return TryStoreBucket();
        }

        return false;
    }

    public bool TryStoreBucket()
    {
        Debug.Log($"Silo.TryStoreBucket: Current={currentStoredBuckets}, Capacity={storageCapacity}");
        if (currentStoredBuckets >= storageCapacity)
        {
            Debug.Log("Silo: Storage is full, cannot store bucket");
            return false;
        }

        currentStoredBuckets++;
        Debug.Log($"Silo: Bucket stored, now {currentStoredBuckets}/{storageCapacity}");
        SpawnBucketVisual();
        UpdateStorageDisplay();
        return true;
    }

    public bool TryRetrieveBucket()
    {
        if (currentStoredBuckets <= 0)
            return false;

        currentStoredBuckets--;
        if (spawnedBuckets.Count > 0)
        {
            GameObject lastBucket = spawnedBuckets[spawnedBuckets.Count - 1];
            spawnedBuckets.RemoveAt(spawnedBuckets.Count - 1);
            Destroy(lastBucket);
        }
        UpdateStorageDisplay();
        return true;
    }

    public int GetStoredBucketCount()
    {
        return currentStoredBuckets;
    }

    private void SpawnBucketVisual()
    {
        if (storedWaterBucketPrefab == null)
        {
            Debug.LogWarning("Silo: Stored Water Bucket Prefab is not assigned!");
            return;
        }

        // Always spawn at Silo's center (0,0)
        Vector3 spawnPos = transform.position;

        Debug.Log($"Silo: Spawning bucket visual at {spawnPos}, parent={transform.name}");
        GameObject bucket = Instantiate(storedWaterBucketPrefab, spawnPos, Quaternion.identity, transform);
        StoredWaterBucket waterBucket = bucket.GetComponent<StoredWaterBucket>();
        
        if (waterBucket == null)
        {
            Debug.Log("Silo: Adding StoredWaterBucket component to spawned bucket");
            waterBucket = bucket.AddComponent<StoredWaterBucket>();
        }
        
        waterBucket.SetSilo(this);
        spawnedBuckets.Add(bucket);
        Debug.Log($"Silo: Bucket spawned successfully, total buckets={spawnedBuckets.Count}");
    }

    private void UpdateStorageDisplay()
    {
        if (storageCountText != null)
        {
            storageCountText.text = $"{currentStoredBuckets}/{storageCapacity}";
        }
    }

    public bool TryUpgradeCapacity()
    {
        int upgradeCost = GetUpgradeCost();
        GoldManager goldManager = GoldManager.Instance;

        if (goldManager == null)
        {
            Debug.LogWarning("Silo: GoldManager not found");
            return false;
        }

        if (!goldManager.TrySpendGold(upgradeCost))
        {
            Debug.Log($"Silo: Not enough gold for upgrade (need {upgradeCost}, have {goldManager.CurrentGold})");
            return false;
        }

        upgradeCount++;
        storageCapacity += upgradeCapacityIncrease;
        Debug.Log($"Silo: Upgraded! Count={upgradeCount}, Capacity={storageCapacity}");
        UpdateStorageDisplay();
        UpdateUpgradePriceDisplay();
        return true;
    }

    public int GetUpgradeCost()
    {
        return baseUpgradeCost + (upgradeCostMultiplier * upgradeCount);
    }

    public int GetStorageCapacity()
    {
        return storageCapacity;
    }

    public int GetUpgradeCount()
    {
        return upgradeCount;
    }

    public void ResetStorage()
    {
        currentStoredBuckets = 0;
        upgradeCount = 0;
        storageCapacity = baseStorageCapacity;
        foreach (GameObject bucket in spawnedBuckets)
        {
            if (bucket != null)
                Destroy(bucket);
        }
        spawnedBuckets.Clear();
        UpdateStorageDisplay();
        UpdateUpgradePriceDisplay();
    }

    private void UpdateUpgradePriceDisplay()
    {
        if (upgradePriceText != null)
        {
            upgradePriceText.text = GetUpgradeCost().ToString();
        }
    }
}
