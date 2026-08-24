using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class StoredWaterBucket : MonoBehaviour
{
    private Silo parentSilo;
    private Vector3 startPosition;
    private Vector3 dragOffset;
    private float dragDepth;
    private bool isDragging;
    private Camera activeCamera;
    private BoxCollider2D bucketCollider;

    private void Awake()
    {
        startPosition = transform.position;
        activeCamera = Camera.main;
        bucketCollider = GetComponent<BoxCollider2D>();
    }

    public void SetSilo(Silo silo)
    {
        parentSilo = silo;
    }

    private void OnMouseDown()
    {
        if (activeCamera == null)
            return;

        isDragging = true;
        dragDepth = activeCamera.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (!isDragging || activeCamera == null)
            return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;
        Vector3 dropWorldPosition = GetMouseWorldPosition();

        if (TryDropWaterOnFarm(dropWorldPosition))
        {
            return;
        }

        // Return to silo if not dropped on valid target
        transform.position = startPosition;
    }

    private bool TryDropWaterOnFarm(Vector3 worldPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in colliders)
        {
            if (hit == bucketCollider)
                continue;

            FarmPlot plot = hit.GetComponentInParent<FarmPlot>();
            if (plot != null && plot.HasField())
            {
                // Apply water fill and show effect
                if (plot.TryFillWater(out Vector3 snapPosition))
                {
                    if (parentSilo != null)
                    {
                        parentSilo.TryRetrieveBucket();
                    }
                    Destroy(gameObject);
                    return true;
                }
            }
        }

        return false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth;
        return activeCamera.ScreenToWorldPoint(mousePosition);
    }
}
