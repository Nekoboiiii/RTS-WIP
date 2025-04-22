using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RadialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnitScriptableObject unitData; // This gets set during button creation
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private string hoverTweenId;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        hoverTweenId = $"Hover_{gameObject.name}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitData == null) return;

        RadialMenu.Instance?.ShowInfo(unitData);
        DOTween.Kill(hoverTweenId);
        rectTransform.DOScale(originalScale * 1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .SetId(hoverTweenId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RadialMenu.Instance?.ScheduleHideInfo();
        DOTween.Kill(hoverTweenId);
        rectTransform.DOScale(originalScale, 0.15f)
            .SetEase(Ease.InBack)
            .SetId(hoverTweenId);
    }
}
