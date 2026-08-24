using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CardUI : MonoBehaviour
{
    [SerializeField] private float moveUpDistance = 0.3f;
    [SerializeField] private float animationDuration = 0.15f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isHovering;
    private Coroutine animationCoroutine;

    private void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        // Sol tık basılıysa animasyonu başlatma (drag işlemi sırasında)
        if (Input.GetMouseButton(0))
            return;

        if (isHovering)
            return;

        isHovering = true;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        Vector3 targetPosition = originalPosition + Vector3.up * moveUpDistance;
        Vector3 targetScale = originalScale; // Sadece position değişsin, scale'i koru

        animationCoroutine = StartCoroutine(AnimateTo(targetPosition, targetScale));
    }

    private void OnMouseExit()
    {
        // Sol tık basılıysa (drag sırasında) animasyon başlatma
        if (Input.GetMouseButton(0))
        {
            isHovering = false;
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            // Kartı original pozisyonuna döndür
            transform.position = originalPosition;
            transform.localScale = originalScale;
            return;
        }

        if (!isHovering)
            return;

        isHovering = false;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateTo(originalPosition, originalScale));
    }

    private IEnumerator AnimateTo(Vector3 targetPosition, Vector3 targetScale)
    {
        float elapsed = 0f;
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = EaseOutQuad(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = targetScale;
        animationCoroutine = null;
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    public void ResetAnimation()
    {
        isHovering = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        transform.position = originalPosition;
        transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
}
