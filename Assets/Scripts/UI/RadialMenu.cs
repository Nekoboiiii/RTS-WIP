using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class RadialMenu : MonoBehaviour
{
    public static RadialMenu Instance;

    public GameObject buttonPrefab;
    public Transform buttonParent; // parent should be in a World Space Canvas
    public float radius = 100f;
    public GameObject infoPanel;
    public TMPro.TextMeshProUGUI infoText;
    public Image infoIcon;

    private Building activeBuilding;
    private Coroutine hideCoroutine;
    private UnitScriptableObject currentlyHoveredUnit;

    void Awake()
    {
        Instance = this;
        infoPanel.SetActive(false); // Hide visual
        gameObject.SetActive(true); // Ensure script runs
        Hide(); // Hide visuals but keep script running
    }

    void OnEnable()
    {
        HideInfoInstant(); // Make sure nothing lingers when opening
    }

    public void PopulateMenu(UnitScriptableObject[] units, Building building)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.LogWarning("RadialMenu is still inactive when PopulateMenu is called!");
        }

        activeBuilding = building;
        ClearMenu();

        Vector3 worldPos = building.transform.position + Vector3.up * 2f;
        transform.position = worldPos;

        float angleStep = 360f / units.Length;

        for (int i = 0; i < units.Length; i++)
        {
            UnitScriptableObject unit = units[i];
            if (unit == null)
            {
                Debug.LogWarning($"Unit at index {i} is null.");
                continue;
            }

            GameObject btn = Instantiate(buttonPrefab, buttonParent);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero; // Start from center

            // Calculate target position
            float angle = i * angleStep;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 targetPos = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;

            // Make sure it faces outward
            rt.localRotation = Quaternion.Euler(0, 0, angle - 90f);

            // Add fade-in effect via CanvasGroup
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            float delay = i * 0.1f;

            // Animate fade in and move out
            cg.DOFade(1, 1.0f).SetDelay(delay);
            rt.DOAnchorPos(targetPos, 0.4f)
              .SetEase(Ease.OutBack)
              .SetDelay(delay);

            // Icon setup
            Transform iconTransform = btn.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null && unit.icon != null)
                {
                    iconImage.sprite = unit.icon;
                    iconImage.enabled = true;
                }
            }

            // Click
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => activeBuilding.TrySpawnUnit(unit));
            }

            // Hover events
            EventTrigger trigger = btn.GetComponent<EventTrigger>() ?? btn.AddComponent<EventTrigger>();
            AddHoverEvents(trigger, unit);
        }
    }

    void AddHoverEvents(EventTrigger trigger, UnitScriptableObject unit)
    {
        RectTransform rect = trigger.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

        EventTrigger.Entry enter = new() { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((data) =>
        {
            ShowInfo(unit);
            rect.DOKill();
            rect.DOScale(originalScale * 1.2f, 0.15f).SetEase(Ease.OutBack);
        });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new() { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((data) =>
        {
            ScheduleHideInfo();
            rect.DOKill();
            rect.DOScale(originalScale, 0.15f).SetEase(Ease.InBack);
        });
        trigger.triggers.Add(exit);
    }

    void ShowInfo(UnitScriptableObject unit)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        currentlyHoveredUnit = unit;
        infoPanel.SetActive(true);

        infoIcon.sprite = unit.icon;
        infoText.text =
            $"<b>{unit.unitName}</b>\n\n" +
            $"<b>Cost:</b>\n" +
            $"- Metal: {unit.metalCost}\n" +
            $"- Stone: {unit.stoneCost}\n" +
            $"- Wood: {unit.woodCost}\n" +
            $"- Gold: {unit.goldCost}\n" +
            $"- Food: {unit.foodCost}\n\n" +
            $"<i>{unit.description}</i>";
    }

    void ScheduleHideInfo()
    {
        if (!gameObject.activeInHierarchy) return;
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideInfoDelayed());
    }

    IEnumerator HideInfoDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HideInfoInstant();
    }

    void HideInfoInstant()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        infoPanel.SetActive(false);
        currentlyHoveredUnit = null;
    }

    void ClearMenu()
    {
        foreach (Transform child in buttonParent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = child.gameObject.AddComponent<CanvasGroup>();
            }

            // Animate position to center, scale down, and fade out
            Vector3 centerPos = Vector3.zero; // Center of radial menu (0, 0)
            cg.DOFade(0f, 0.8f).SetEase(Ease.InOutSine); // Fade out
            child.DOScale(0f, 0.25f).SetEase(Ease.InBack); // Shrink the buttons
            child.DOMove(centerPos, 0.25f).SetEase(Ease.InOutBack); // Move to center

            // Destroy after animation finishes
            child.DOScale(0f, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(child.gameObject));
        }

        HideInfoInstant();
    }



    public void Show()
    {
        gameObject.SetActive(true);
        HideInfoInstant();

        // Optional: pulse scale instead of rotate
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        // Perform radial collapse before hiding
        RadialCollapseAnimation();

        // Deactivate the menu after the animation finishes
        StartCoroutine(DelayedHide());
    }

    IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.3f); // Match animation duration
        gameObject.SetActive(false); // Hide the radial menu
    }

    void RadialCollapseAnimation()
    {
        foreach (Transform child in buttonParent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = child.gameObject.AddComponent<CanvasGroup>();
            }

            // Animate position to center, scale down, and fade out
            Vector3 centerPos = Vector3.zero; // Center of radial menu (0, 0)
            cg.DOFade(0f, 0.25f).SetEase(Ease.InOutSine); // Fade out
            child.DOScale(0f, 0.25f).SetEase(Ease.InBack); // Shrink the buttons
            child.DOMove(centerPos, 0.25f).SetEase(Ease.InOutBack); // Move to center
        }
    }


}
