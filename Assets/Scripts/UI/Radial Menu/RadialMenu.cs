using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;
using static TweenHelper;
using UnityEngine.Analytics;

public class RadialMenu : MonoBehaviour
{
    [Header("Radial Menu")]
    public static RadialMenu Instance;
    public GameObject buttonPrefab;
    public Transform buttonParent; // parent should be in a World Space Canvas
    public float radius = 100f;

    [Header("Info Panel")]
    public GameObject infoPanel;
    public TMPro.TextMeshProUGUI infoText;
    public Image infoIcon;

    // Private variables
    [SerializeField] private bool isCancelling = false; // Flag to check if the menu is currently cancelling
    [SerializeField] private bool isAnimating = false; // Flag to check if the menu is currently animating
    private Building activeBuilding;
    private Building currentBuilding;
    private Coroutine hideCoroutine;
    private Coroutine cancelCoroutine; // Flag to avoid using StopAllCoroutines() because it is to aggressive
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

    private bool HasVisibleButtons()
    {
        foreach (Transform child in buttonParent)
        {   
            if (child != null && child.gameObject.activeInHierarchy)
            {
                return true;
            }
        }            
        return false;
    }

    public void PopulateMenu(RadialMenuEntry[] entries, Building building)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.LogWarning("RadialMenu is still inactive when PopulateMenu is called!");
        }

        if (currentBuilding == building && HasVisibleButtons())
        {
            Debug.LogWarning("RadialMenu already populated for this building.");
            return;
        }

        currentBuilding = building;
        activeBuilding = building;

        // Prevents reopening if it is still animating
        if (isAnimating || isCancelling)
        {
            Debug.LogWarning("RadialMenu is animating, skipping reopen!");
            return;
        }
        
        ClearMenu();
        KillMenuTweens();

        Vector3 worldPos = building.transform.position + Vector3.up * 2f;
        transform.position = worldPos;

        float angleStep = 360f / entries.Length;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry == null)
            {
                Debug.LogWarning($"Entry at index {i} is null.");
                continue;
            }

            // Creates the button and sets its parent
            GameObject btn = Instantiate(buttonPrefab, buttonParent);
            btn.SetActive(false); // Start inactive to prevent flicker
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero; // Start from center

            // Calculate target position
            float angle = i * angleStep;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 targetPos = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;

            // Set button rotation
            rt.localRotation = Quaternion.Euler(0, 0, angle - 90f);

            // Makes Sure no animations overlap occurs
            rt.DOKill(true); // Kill any previous animations
            rt.localScale = Vector3.one; // Reset the scale just in case anything was changed
            rt.anchoredPosition = Vector2.zero; // Reset the position back to center

            // Add fade-in effect via CanvasGroup
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.AddComponent<CanvasGroup>();  // This is already done, so check if it's failing
            cg.alpha = 0;
            cg.DOKill(true);
            isAnimating = true; // Set animation state
            cg.blocksRaycasts = false; // Prevents raycast during animation

            // Click logic
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false; // Prevents clicks until animation is done

                var radialButton = btn.GetComponent<RadialButton>() ?? btn.AddComponent<RadialButton>();

                if (entry.unitToSpawn != null)
                {
                    var unit = entry.unitToSpawn;
                    button.onClick.AddListener(() => building.TrySpawnUnit(unit));
                    radialButton.unitData = unit;
                }
                else if (entry.onClickFallback != null)
                {
                    button.onClick.AddListener(() => entry.onClickFallback.Invoke());
                    radialButton.unitData = null; // optional: can also skip assigning if fallback
                }
            }


            // Animate using a Sequence
            float delay = i * 0.1f;
            Sequence seq = DOTween.Sequence().WithButtonId(i).WithMenuId();
            seq.AppendInterval(delay); // Wait before starting animation
            seq.AppendCallback(() =>
            {
                cg.alpha = 0f; // Still invisible, but now its ready to fade
                cg.blocksRaycasts = false;
                btn.SetActive(true);
            });
            seq.Append(cg.DOFade(1, 0.4f).WithButtonId(i));
            seq.Join(rt.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutBack).WithButtonId(i));
            seq.OnComplete(() =>
            {
                // Check if button is still valid before completing the tween
                if (btn != null && btn.activeInHierarchy)
                {
                    cg.blocksRaycasts = true; // Enable raycast after animation
                    if (button != null && button.gameObject.activeInHierarchy)
                    {
                        button.interactable = true; // Enable button interaction
                    }
                }
            });

            // Icon setup
            Transform iconTransform = btn.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null && entry.icon != null)
                {
                    iconImage.sprite = entry.icon;
                    iconImage.color = Color.white; // Just in case alpha is 0
                    iconImage.enabled = true;
                }
                else
                {
                    Debug.Log($"iconImage is {(iconImage == null ? "null" : "OK")}, entry.icon is {(entry.icon == null ? "null" : entry.icon.name)}");
                }
            }
            else
            {
                Debug.LogWarning("Icon transform not found on button prefab!");
            }

        }

        // Reset the isAnimating state after all buttons are animated
        DOVirtual.DelayedCall(entries.Length * 0.1f + 0.5f, () =>
        {
            isAnimating = false;
        });
    }

    public void ShowInfo(UnitScriptableObject unit)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        currentlyHoveredUnit = unit;
        infoPanel.SetActive(true);

        RectTransform rt = infoPanel.GetComponent<RectTransform>();

        // Clamp logic 
        float canvasWidth = rt.parent.GetComponent<RectTransform>().rect.width;
        float canvasHeight = rt.parent.GetComponent<RectTransform>().rect.height;

        float x = 0f; // Center
        float y = Mathf.Clamp(50f, 0f, canvasHeight - rt.rect.height); // 50f above the bottom of the screen
        rt.anchoredPosition = new Vector2(x, y);
        
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

    public void ScheduleHideInfo()
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
            // Check if the child exists and is active
            if (child != null && child.gameObject.activeInHierarchy)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                CanvasGroup cg = child.GetComponent<CanvasGroup>();

                string clearMenuTweenId = $"RadialMenu_Clear_{child.gameObject.GetInstanceID()}";

                // If either the RectTransform or CanvasGroup is missing, skip this child
                if (rt != null && cg != null)
                {
                    // Create a CanvasGroup if it doesn't exist
                    cg = cg ?? child.gameObject.AddComponent<CanvasGroup>();

                    // Start the tween animations
                    Sequence s = DOTween.Sequence().SetId(clearMenuTweenId);

                    // Only apply tweens if the object is still valid
                    s.Append(rt.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.InOutBack).OnKill(() =>
                    {
                        // Check if the object is still valid after the tween
                        if (child != null && child.gameObject.activeInHierarchy)
                        {
                            rt.gameObject.SetActive(false); // Safely disable object if it's valid
                        }
                    }));

                    s.Join(rt.DOScale(0f, 0.25f).SetEase(Ease.InBack)); // Shrink the buttons
                    s.Join(cg.DOFade(0f, 0.25f).SetEase(Ease.InOutSine)); // Fade out

                    s.OnComplete(() =>
                    {
                        // Before destroying, check if it's still valid
                        if (child != null && child.gameObject.activeInHierarchy)
                        {
                            Destroy(child.gameObject); // Destroy the object safely if it's still active
                        }
                    });
                }
            }
        }

        // Additional logic
        HideInfoInstant();
        currentlyHoveredUnit = null; // Reset the currently hovered unit
    }

    public void Show()
    {

        if(isAnimating) return; // Prevent multiple calls
        isAnimating = true;

        gameObject.SetActive(true);
        HideInfoInstant();

        // Optional: pulse scale instead of rotate
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() => isAnimating = false);
    }

    public void Hide()
    {  
        Clear();
        
        if(isAnimating) return; // Prevent multiple calls
        isAnimating = true;

        // Perform radial collapse before hiding
        RadialCollapseAnimation();

        // Deactivate the menu after the animation finishes
        StartCoroutine(DelayedHide());
    }

    IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.4f); // Match animation duration
        gameObject.SetActive(false); // Hide the radial menu
        isAnimating = false; // Reset animation state
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

            string radialMenuCollapseTweenId = $"RadialMenu_Collapse_{child.gameObject.GetInstanceID()}"; 

            // Animate position to center, scale down, and fade out
            Vector3 centerPos = Vector3.zero; // Center of radial menu (0, 0)
            cg.DOFade(0f, 0.25f).SetEase(Ease.InOutSine).SetId(radialMenuCollapseTweenId); // Fade out
            child.DOScale(0f, 0.25f).SetEase(Ease.InBack).SetId(radialMenuCollapseTweenId); // Shrink the buttons
            child.DOMove(centerPos, 0.25f).SetEase(Ease.InOutBack).SetId(radialMenuCollapseTweenId); // Move to center
        }
    }

    public void Cancel()
    {

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (cancelCoroutine != null)
        {
            StopCoroutine(cancelCoroutine);
            cancelCoroutine = null;
        }

        // Even if already cancelling, still allow recovery 
        // DO NOT DELETE BREAKS THE WHOLE MENU IF DELETED
        if (isCancelling || isAnimating)
        {
            Debug.Log("[RadialMenu] Cancel called but was already cancelling. Restarting cancel flow.");
        }

        isCancelling = true;
        isAnimating = true;

        foreach (Transform child in buttonParent)
        {
            if (child == null || !child.gameObject.activeInHierarchy) continue;

            string cancelTweenId = $"RadialMenu_Cancel_{child.gameObject.GetInstanceID()}";

            RectTransform rt = child.GetComponent<RectTransform>();
            CanvasGroup cg = child.GetComponent<CanvasGroup>() ?? child.gameObject.AddComponent<CanvasGroup>();

            rt.DOKill();
            cg.DOKill();

            Sequence s = DOTween.Sequence().SetId(cancelTweenId);
            s.Join(rt.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.InOutBack));
            s.Join(rt.DOScale(0f, 0.25f).SetEase(Ease.InBack));
            s.Join(cg.DOFade(0f, 0.25f).SetEase(Ease.InOutSine));

            s.OnComplete(() =>
            {
                if (child != null && child.gameObject.activeInHierarchy)
                    Destroy(child.gameObject);
            });
        }

        DOTween.Kill(transform); // Kills transform tweens
        KillMenuTweens(); // Kill all global radial menu animations
        HideInfoInstant();

        cancelCoroutine = StartCoroutine(DelayedCancelCleanup());
    }



    IEnumerator DelayedCancelCleanup()
    {
        yield return new WaitForSeconds(0.3f); // wait for animation to cleanly finish

        isAnimating = false;
        isCancelling = false;
        Clear();
        cancelCoroutine = null;
        gameObject.SetActive(false);
    }

    public void Clear()
    {
        KillMenuTweens();

        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        currentBuilding = null;
    }
}
