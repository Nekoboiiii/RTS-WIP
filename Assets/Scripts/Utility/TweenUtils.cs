using UnityEngine;
using DG.Tweening;

public static class TweenHelper
{
    private const string GlobalMenuId = "RadialMenu_Animation";

    // Global ID
    public static Tweener WithMenuId(this Tweener tween)
    {
        return tween.SetId(GlobalMenuId);
    }

    public static Sequence WithMenuId(this Sequence sequence)
    {
        return sequence.SetId(GlobalMenuId);
    }

    public static void KillMenuTweens()
    {
        DOTween.Kill(GlobalMenuId);
    }

    // Per Button ID
    public static Tweener WithButtonId(this Tweener tween, int index)
    {
        return tween.SetId(GetButtonId(index));
    }

    public static Sequence WithButtonId(this Sequence sequence, int index)
    {
        return sequence.SetId(GetButtonId(index));
    }

    public static void KillButtonTweens(int index)
    {
        DOTween.Kill(GetButtonId(index));
    }

    private static string GetButtonId(int index)
    {
        return $"RadialMenu_Button_{index}";
    }
}