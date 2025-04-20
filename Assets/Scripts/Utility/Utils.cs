using UnityEngine;

public static class Utils
{
    public static Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        return new Rect(
            Mathf.Min(start.x, end.x),
            Screen.height - Mathf.Max(start.y, end.y),
            Mathf.Abs(start.x - end.x),
            Mathf.Abs(start.y - end.y)
        );
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
    
    public static Bounds GetViewportBounds(Camera camera, Vector2 screenPosition1, Vector2 screenPosition2)
    {
        Vector3 v1 = camera.ScreenToViewportPoint(screenPosition1);
        Vector3 v2 = camera.ScreenToViewportPoint(screenPosition2);
        Vector3 min = Vector3.Min(v1, v2);
        Vector3 max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        return new Bounds((min + max) / 2, max - min);
    }
}
