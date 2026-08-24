using UnityEngine;

[RequireComponent(typeof(CardsMovement))]
[RequireComponent(typeof(BoxCollider2D))]
public class BucketCard : MonoBehaviour, ICardDropProcessor
{
    [SerializeField] private int price = 1;

    private CardsMovement movement;
    private BoxCollider2D ownCollider;

    private void Awake()
    {
        movement = GetComponent<CardsMovement>();
        ownCollider = GetComponent<BoxCollider2D>();
    }

    public bool TryHandleDrop(Vector3 worldPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in colliders)
        {
            if (hit == ownCollider)
            {
                continue;
            }

            // Check Silo first
            Silo silo = hit.GetComponentInParent<Silo>();
            if (silo != null)
            {
                Debug.Log($"BucketCard: Silo found at {silo.transform.name}, attempting to store bucket at {worldPosition}");
                if (silo.TryStoreBucket())
                {
                    Debug.Log("BucketCard: Bucket stored successfully");
                    GoldManager.Instance?.ConsumeGold(price);
                    CardHandManager.Instance?.NotifyCardUsed();
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log("BucketCard: Silo is full");
                }
            }

            FarmPlot plot = hit.GetComponentInParent<FarmPlot>();

            if (plot != null && !plot.CanFillWater())
            {
                continue;
            }

            if (GoldManager.Instance == null || !GoldManager.Instance.TrySpendGold(price))
            {
                return false;
            }

            if (plot != null && plot.TryFillWater(out Vector3 snapPosition))
            {
                movement.SnapToPosition(snapPosition);
                CardHandManager.Instance?.NotifyCardUsed();
                Destroy(gameObject);
                return true;
            }

            GoldManager.Instance?.AddGold(price);
        }

        return false;
    }
}
