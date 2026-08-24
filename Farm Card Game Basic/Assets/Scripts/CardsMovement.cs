using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class CardsMovement : MonoBehaviour
{
    [SerializeField] private float returnSpeed = 15f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D cardCollider;
    private Camera activeCamera;
    private Vector3 startPosition;
    private Vector3 dragOffset;
    private float dragDepth;
    private bool isDragging;
    private Coroutine returnRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardCollider = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
        activeCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (activeCamera == null)
        {
            return;
        }

        isDragging = true;
        dragDepth = activeCamera.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - GetMouseWorldPosition();

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        // CardUI animasyonlarını sıfırla
        CardUI cardUI = GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.ResetAnimation();
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || activeCamera == null)
        {
            return;
        }

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        Vector3 dropWorldPosition = GetMouseWorldPosition();

        if (TryDropOnTrash(dropWorldPosition))
        {
            return;
        }

        ICardDropProcessor dropProcessor = GetComponent<ICardDropProcessor>();

        if (dropProcessor == null || !dropProcessor.TryHandleDrop(dropWorldPosition))
        {
            returnRoutine = StartCoroutine(ReturnToStartPosition());
        }
    }

    public void SetStartPosition(Vector3 newStartPosition)
    {
        startPosition = newStartPosition;
    }

    public void SnapToPosition(Vector3 position)
    {
        transform.position = position;
        startPosition = position;
    }

    private IEnumerator ReturnToStartPosition()
    {
        while (Vector3.Distance(transform.position, startPosition) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                returnSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = startPosition;
        returnRoutine = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth;

        return activeCamera.ScreenToWorldPoint(mousePosition);
    }

    private bool TryDropOnTrash(Vector3 worldPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D hit = colliders[i];
            if (hit == null || hit == cardCollider)
            {
                continue;
            }

            TrashZone trashZone = hit.GetComponentInParent<TrashZone>();
            if (trashZone == null)
            {
                continue;
            }

            CardHandManager.Instance?.NotifyCardUsed();
            Destroy(gameObject);
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardCollider = GetComponent<BoxCollider2D>();
    }
#endif
}
