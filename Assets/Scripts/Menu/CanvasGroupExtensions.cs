using UnityEngine;

public static class CanvasGroupExtensions
{
    public static void SetVisible(this CanvasGroup cg, bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }
}