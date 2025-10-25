// Assets/2_Scripts/ButtonSound.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public AudioClip hoverClip;
    public AudioClip clickClip;
    [Range(0.9f, 1.2f)] public float hoverScale = 1.05f;
    Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null) GameManager.Instance?.PlaySfx(hoverClip);
        StopAllCoroutines();
        StartCoroutine(ScaleTo(hoverScale, 0.08f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(1f, 0.08f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickClip != null) GameManager.Instance?.PlaySfx(clickClip);
        StartCoroutine(ClickAnim());
    }

    IEnumerator ScaleTo(float target, float duration)
    {
        Vector3 start = transform.localScale;
        Vector3 end = originalScale * target;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        transform.localScale = end;
    }

    IEnumerator ClickAnim()
    {
        yield return ScaleTo(0.95f, 0.03f);
        yield return ScaleTo(1f, 0.06f);
    }
}
