using System;
using UnityEngine;
using UnityEngine.Diagnostics;
[Obsolete("This class is deprecated. Use SelectionManager instead.")]

public class SelectionBoxDrawer : MonoBehaviour
{
    public Vector2 startPos;
    public bool isSelecting;

    void OnGUI()
    {
       if (isSelecting)
       {
        var rect = Utils.GetScreenRect(startPos, Input.mousePosition);
        Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
        Utils.DrawScreenRectBorder(rect, 2, Color.white);
       } 
    }
}
