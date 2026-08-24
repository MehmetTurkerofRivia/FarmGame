using UnityEngine;

[RequireComponent(typeof(CardsMovement))]
[RequireComponent(typeof(BoxCollider2D))]
public class FieldCard : MonoBehaviour, ICardDropProcessor
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

            FarmPlot plot = hit.GetComponentInParent<FarmPlot>();

            if (plot == null)
            {
                continue;
            }

            if (!plot.CanPlaceField())
            {
                // already has a field, skip this plot
                continue;
            }

            if (GoldManager.Instance == null || !GoldManager.Instance.TrySpendGold(price))
            {
                return false;
            }

            if (plot.TryPlaceField(out Vector3 snapPosition))
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
