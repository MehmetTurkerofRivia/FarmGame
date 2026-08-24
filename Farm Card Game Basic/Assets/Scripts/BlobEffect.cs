using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlobEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleUpFactor = 1.15f;
    [SerializeField] private float animationDuration = 0.12f;
    [SerializeField] private float pulseDuration = 0.09f;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayScaleTo(originalScale * scaleUpFactor, animationDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScaleTo(originalScale, animationDuration);
    }

    public void PlayPulse()
    {
        if (!isActiveAndEnabled)
            return;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(PulseRoutine());
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private IEnumerator PulseRoutine()
    {
        yield return AnimateTo(originalScale * scaleUpFactor, pulseDuration);
        yield return AnimateTo(originalScale, pulseDuration);
        animationCoroutine = null;
    }

    private void PlayScaleTo(Vector3 target, float duration)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateTo(target, duration));
    }

    private IEnumerator AnimateTo(Vector3 target, float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = EaseOutQuad(t);
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        transform.localScale = originalScale;
    }
}
