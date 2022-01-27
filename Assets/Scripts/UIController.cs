using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    public IEnumerator FadeIn(CanvasGroup container, float duration)
    {
        container.interactable = false;
        container.gameObject.SetActive(true);
        container.alpha = 0f;
        yield return new WaitForSeconds(0);
        container.DOFade(1f, duration).SetUpdate(true);
        container.interactable = true;
    }

    public IEnumerator FadeOut(CanvasGroup container, float duration)
    {
        container.interactable = false;
        container.alpha = 1f;
        container.DOFade(0f, duration).SetEase(Ease.InQuint);
        yield return new WaitForSeconds(duration);
        container.gameObject.SetActive(false);
        container.interactable = true;
    }

    public IEnumerator AnimationWideIn(RectTransform container, float duration)
    {
        container.localScale = new Vector3(1, 1, 1);
        container.DOScale(new Vector3(0f, 1f, 1f), duration).SetUpdate(true);
        yield return new WaitForSeconds(duration);
        container.gameObject.SetActive(false);
    }

    public IEnumerator AnimationWideOut(RectTransform container, float duration)
    {
        container.localScale = new Vector3(0, 1, 1);
        container.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0);
        container.DOScale(new Vector3(1f, 1f, 1f), duration).SetUpdate(true);
        container.DOAnchorPosY(0f, duration).SetUpdate(true);
    }

    public IEnumerator AnimationSnapOut(RectTransform container, float duration)
    {
        container.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        container.gameObject.SetActive(true);
        yield return new WaitForSeconds(0);
        container.DOScale(new Vector3(1f, 1f, 1f), duration).SetUpdate(true);
        container.DOAnchorPosY(0f, duration).SetUpdate(true);
    }

    public IEnumerator AnimationSnapIn(RectTransform container, float duration)
    {
        container.localScale = new Vector3(1, 1, 1);
        container.DOScale(new Vector3(0.7f, 0.7f, 0.7f), duration).SetUpdate(true);
        container.DOAnchorPosY(-200, duration).SetUpdate(true);
        yield return new WaitForSeconds(duration);
        container.gameObject.SetActive(false);
    }
}
