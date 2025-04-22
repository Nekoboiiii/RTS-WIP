using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Obsolete("Use the new Selection Scripts")]

public class SelectionManager : MonoBehaviour
{
    public LayerMask selectableLayer;
    private Vector2 startPos;
    private bool isSelecting = false;
    private bool hasBuilding = false;
    public RadialMenu radialMenu;
    public List<Selectable> selectedObjects = new();
    private float moveCommandTimer = 0f;
    public float moveCommandInterval = 0.1f;

    void Update()
    {
        bool isMultiSelect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isSelecting = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            Vector2 endPos = Input.mousePosition;

            float clickThreshold = 5f; // Threshold for detecting a click vs drag
            if (Vector2.Distance(startPos, endPos) < clickThreshold)
            {
                // Single click select, passing the isMultiSelect value to handle multi-selection
                TrySingleClickSelect(isMultiSelect);
            }
            else
            {
                // Drag box selection
                SelectObjectsInDragArea();
            }
        }

        HandleRightClick();
    }


    void OnGUI() // Selection box drawing
    {
       if (isSelecting)
       {
        var rect = Utils.GetScreenRect(startPos, Input.mousePosition);
        Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
        Utils.DrawScreenRectBorder(rect, 2, Color.white);
       } 
    }

    void SelectObjectsInDragArea()
    {
        var viewportBounds = Utils.GetViewportBounds(Camera.main, startPos, Input.mousePosition);
        bool foundSomething = false;
        

        List<Selectable> objectsToSelect = new List<Selectable>();

        foreach (var selectable in FindObjectsByType<Selectable>(FindObjectsSortMode.None))
        {
            Vector3 screenPos = Camera.main.WorldToViewportPoint(selectable.transform.position);
            if (viewportBounds.Contains(screenPos))
            {
                objectsToSelect.Add(selectable);
                if (selectable.building != null) hasBuilding = true;
                foundSomething = true;
            }
        }

        if (foundSomething)
        {
            if (hasBuilding)
            {
                DeselectAll();
                foreach (var selectable in objectsToSelect)
                {
                    if (selectable.building != null)
                    {
                        SelectObject(selectable);
                        break;
                    }
                }
            }
            else
            {
                foreach (var selectable in objectsToSelect)
                {
                    SelectObject(selectable);
                }
            }
        }
    }

    void TrySingleClickSelect(bool isMultiSelect)
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        if (hit != null)
        {
            Selectable selectable = hit.GetComponent<Selectable>();
            if (selectable != null)
            {
                if (!isMultiSelect)
                {
                    // Single selection: Deselect all first, then select the clicked object
                    DeselectAll();
                    SelectObject(selectable);
                }
                else
                {
                    // Multi-selection (Shift/Ctrl pressed): Toggle the selection
                    if (selectedObjects.Contains(selectable))
                    {
                        // If already selected, deselect it
                        selectable.Deselect();
                        selectedObjects.Remove(selectable);
                    }
                    else
                    {
                        // Otherwise, select it
                        SelectObject(selectable);
                    }
                }
            }
        }
        else
        {
            // Deselect all if clicking on an empty area (nothing hit)
            DeselectAll();
        }
    }



    void HandleRightClick()
    {
        if (Input.GetMouseButton(1) && selectedObjects.Count > 0)
        {
            moveCommandTimer += Time.deltaTime;
            if (moveCommandTimer >= moveCommandInterval)
            {
                moveCommandTimer = 0f;

                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 targetPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                RaycastHit2D hit = Physics2D.Raycast(targetPos2D, Vector2.zero);
                if (hit.collider == null)
                {
                    foreach (var selectable in selectedObjects)
                    {
                        if (selectable.unit != null)
                        {
                            selectable.unit.MoveTo(targetPos2D);
                        }
                    }
                }
            }
        }
        else
        {
            moveCommandTimer = moveCommandInterval;
        }
    }

    private void SelectObject(Selectable selectable)
    {
        if (selectedObjects.Contains(selectable)) return;

        selectable.Select();
        selectedObjects.Add(selectable);

        if (selectable.building != null)
        {
            StartCoroutine(ShowRadialMenuDelayed(selectable.building));
        }
    }

    private IEnumerator ShowRadialMenuDelayed(Building building)
    {
        RadialMenu.Instance.gameObject.SetActive(true);
        yield return null;

        if (building.buildingData != null && building.buildingData.spawnableUnits != null)
        {
            // Convert UnitScriptableObject[] to List<RadialMenuEntry>
            List<RadialMenuEntry> entries = new();

            foreach (var unit in building.buildingData.spawnableUnits)
            {
                if (unit == null) continue;

                // Create a new instance of RadialMenuEntry
                RadialMenuEntry entry = ScriptableObject.CreateInstance<RadialMenuEntry>();
                entry.unitToSpawn = unit;
                entry.onClickFallback = null; // No fallback needed since we want to spawn units
                entry.icon = unit.icon; 

                entries.Add(entry);
            }

            RadialMenu.Instance.PopulateMenu(entries.ToArray(), building);
        }
        else
        {
            Debug.LogWarning("Building data or spawnable units is missing!");
        }
    }


    public void DeselectAll()
    {
        bool hasBuildingSelected = false;

        // Safety check to ensure RadialMenu is not null and is active before calling Cancel       
        if (RadialMenu.Instance != null && RadialMenu.Instance.gameObject.activeInHierarchy)
        {
            RadialMenu.Instance.Cancel(); 
        }

        // Check if any selected object is building
        foreach (var obj in selectedObjects)
        {
            if (obj.building != null)
            {
                hasBuildingSelected = true;
                break;
            }
        }

        // Cancel RadialMenu Animation
        if(hasBuildingSelected && RadialMenu.Instance != null)
        {
            RadialMenu.Instance.Cancel();
        }

        // Deselect all selected objects
        foreach (var obj in selectedObjects)
        {
            obj.Deselect();
        }
        selectedObjects.Clear();
    }
}
