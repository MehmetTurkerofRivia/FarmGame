using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class FarmPlot : MonoBehaviour
{
    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private Vector3 fieldLocalOffset = Vector3.zero;
    [SerializeField] private int maxWorkers = 3;
    [SerializeField] private int maxWater = 8;
    [SerializeField] private Vector2 workerOffsetStep = new Vector2(0.35f, 0.2f);
    [SerializeField] private GameObject workerVisualPrefab;
    [SerializeField] private Vector3 workerMoveAreaLocalOffset = new Vector3(0f, -0.1f, 0f);
    [SerializeField] private Vector2 workerMoveAreaSize = new Vector2(1.6f, 0.8f);

    private SpriteRenderer plotRenderer;
    private BoxCollider2D plotCollider;
    private bool hasField;
    private GameObject fieldInstance;
    
    private int workerCount;
    private float waterCount;
    private TMP_Text fieldWorkerText;
    private Slider fieldWaterSlider;
    private readonly List<GameObject> workerVisualInstances = new List<GameObject>();

    private static readonly List<FarmPlot> AllPlots = new List<FarmPlot>();

    private void Awake()
    {
        plotRenderer = GetComponent<SpriteRenderer>();
        plotCollider = GetComponent<BoxCollider2D>();

        // initial state
        hasField = false;
        workerCount = 0;
        waterCount = 0;
        if (!AllPlots.Contains(this)) AllPlots.Add(this);
    }

    private void OnDestroy()
    {
        if (AllPlots.Contains(this)) AllPlots.Remove(this);
    }

    // Place a field on this plot. Returns true and snapPosition when successful.
    public bool TryPlaceField(out Vector3 snapPosition)
    {
        snapPosition = transform.position;

        if (hasField)
        {
            return false;
        }

        hasField = true;
        workerCount = 0;
        waterCount = maxWater;
        ClearWorkerVisuals();
        SpawnFieldInstance();
        UpdateWorkerText();
        UpdateWaterSlider();
        return true;
    }

    // Add a worker to the field (max workers enforced)
    public bool TryAddWorker(out Vector3 snapPosition)
    {
        snapPosition = transform.position;

        if (!hasField || workerCount >= maxWorkers)
        {
            return false;
        }

        workerCount = Mathf.Min(maxWorkers, workerCount + 1);
        SpawnWorkerVisual();
        UpdateWorkerText();
        snapPosition = transform.position + new Vector3(
            workerOffsetStep.x * workerCount,
            workerOffsetStep.y * workerCount,
            -0.01f * workerCount);
        return true;
    }

    // Fill water by +5 units (clamped to maxWater)
    public bool TryFillWater(out Vector3 snapPosition)
    {
        snapPosition = transform.position;

        if (!hasField)
        {
            return false;
        }

        if (waterCount >= maxWater)
        {
            // already full
            return false;
        }

        waterCount = Mathf.Clamp(waterCount + 5f, 0f, maxWater);
        UpdateWaterSlider();
        snapPosition = transform.position + new Vector3(0f, 0.55f, -0.02f);
        return true;
    }

    // Decrease water by the current plot consumption (base 1, +1 per active worker) at end of turn
    public void DecreaseWaterByOne()
    {
        if (!hasField)
            return;

        float waterConsumption = 1f + workerCount;
        waterCount -= waterConsumption;

        if (waterCount < 0f)
        {
            RemoveField();
            return;
        }

        UpdateWaterSlider();
    }

    // Decrease water on all registered plots by one
    public static void DecreaseWaterForAllPlots()
    {
        for (int i = 0; i < AllPlots.Count; i++)
        {
            FarmPlot plot = AllPlots[i];
            if (plot != null)
                plot.DecreaseWaterByOne();
        }
    }

    public bool HasField()
    {
        return hasField;
    }

    public bool CanPlaceField()
    {
        return !hasField;
    }

    public bool CanAddWorker()
    {
        return hasField && workerCount < maxWorkers;
    }

    public bool CanFillWater()
    {
        return hasField && waterCount < maxWater;
    }

    public int GetWorkerCount()
    {
        return hasField ? workerCount : 0;
    }

    public static int GetTotalWorkerCount()
    {
        int totalWorkerCount = 0;

        for (int i = 0; i < AllPlots.Count; i++)
        {
            FarmPlot plot = AllPlots[i];
            if (plot != null)
            {
                totalWorkerCount += plot.GetWorkerCount();
            }
        }

        return totalWorkerCount;
    }

    public static int GetTotalFieldCount()
    {
        int totalFieldCount = 0;

        for (int i = 0; i < AllPlots.Count; i++)
        {
            FarmPlot plot = AllPlots[i];
            if (plot != null && plot.hasField)
            {
                totalFieldCount++;
            }
        }

        return totalFieldCount;
    }

    public static void ResetAllPlots()
    {
        for (int i = 0; i < AllPlots.Count; i++)
        {
            FarmPlot plot = AllPlots[i];
            if (plot != null)
            {
                plot.RemoveField();
            }
        }
    }

    private void RemoveField()
    {
        hasField = false;
        workerCount = 0;
        waterCount = 0;
        ClearWorkerVisuals();

        if (fieldInstance != null)
        {
            Destroy(fieldInstance);
            fieldInstance = null;
        }

        fieldWorkerText = null;
        fieldWaterSlider = null;
    }

    private void SpawnWorkerVisual()
    {
        if (workerVisualPrefab == null)
        {
            return;
        }

        Vector3 areaCenter = transform.position + workerMoveAreaLocalOffset;
        Vector2 halfArea = workerMoveAreaSize * 0.5f;
        Vector3 randomOffset = new Vector3(
            Random.Range(-halfArea.x, halfArea.x),
            Random.Range(-halfArea.y, halfArea.y),
            -0.01f * (workerVisualInstances.Count + 1));

        GameObject workerVisual = Instantiate(workerVisualPrefab, areaCenter + randomOffset, Quaternion.identity, transform);
        WorkerWander wander = workerVisual.GetComponent<WorkerWander>();

        if (wander == null)
        {
            wander = workerVisual.AddComponent<WorkerWander>();
        }

        wander.SetMoveArea(areaCenter, workerMoveAreaSize);
        workerVisualInstances.Add(workerVisual);
    }

    private void ClearWorkerVisuals()
    {
        for (int i = 0; i < workerVisualInstances.Count; i++)
        {
            GameObject workerVisual = workerVisualInstances[i];
            if (workerVisual != null)
            {
                Destroy(workerVisual);
            }
        }

        workerVisualInstances.Clear();
    }

    private void SpawnFieldInstance()
    {
        if (fieldPrefab == null)
        {
            Debug.LogWarning("Field prefab not assigned on FarmPlot.");
            return;
        }

        if (fieldInstance != null)
        {
            return;
        }

        fieldInstance = Instantiate(fieldPrefab, transform);
        fieldInstance.transform.localPosition = fieldLocalOffset;
        fieldInstance.transform.localRotation = Quaternion.identity;
        // Find TMP and Slider inside the instantiated field prefab
        fieldWorkerText = fieldInstance.GetComponentInChildren<TMP_Text>();
        fieldWaterSlider = fieldInstance.GetComponentInChildren<Slider>();

        if (fieldWorkerText == null)
        {
            Debug.LogWarning("Field prefab does not contain a TMP_Text for worker count.");
        }

        if (fieldWaterSlider == null)
        {
            Debug.LogWarning("Field prefab does not contain a Slider for water display.");
        }

        UpdateWorkerText();
        UpdateWaterSlider();
    }

    private void UpdateWorkerText()
    {
        if (fieldWorkerText != null)
        {
            fieldWorkerText.text = $"{workerCount}/{maxWorkers}";
        }
    }

    private void UpdateWaterSlider()
    {
        if (fieldWaterSlider != null)
        {
            fieldWaterSlider.gameObject.SetActive(hasField);
            fieldWaterSlider.maxValue = Mathf.Max(1, maxWater);
            fieldWaterSlider.value = waterCount;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        plotRenderer = GetComponent<SpriteRenderer>();
        plotCollider = GetComponent<BoxCollider2D>();
    }
#endif
}
