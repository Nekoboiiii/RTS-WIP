using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectingPhase : ISelectionPhase
{
    private SelectionPhaseContext context;
    private Vector2 startPos;
    private bool isSelecting = false;
    private bool isFormationDrag = false; // Track if we're dragging to form a formation
    private Vector2 startMousePosForFormation;
    private float dragThreshold = 10f; // Drag threshold for starting formation

    public void Enter(SelectionPhaseContext ctx)
    {
        context = ctx;
    }

    public void Exit()
    {
        Debug.Log($"[Phase] Exited: {GetType().Name}");
        isSelecting = false;
    }

    public void Update()
    {
        // Ensure left-click selection happens before formation logic
        if (Input.GetMouseButtonDown(0)) // Left-click to start selection
        {
            if (!isFormationDrag) // Ensure right-click drag doesn't interfere with selection
            {
                startPos = Input.mousePosition;
                isSelecting = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isSelecting) // Left-click release
        {
            isSelecting = false;
            float clickThreshold = 15f;
            Vector2 endPos = Input.mousePosition;

            if (Vector2.Distance(startPos, endPos) < clickThreshold)
            {
                TrySingleClickSelect();
            }
            else
            {
                DragSelect();
            }

            context.StartCoroutine(SwitchToIdle()); // Switch back to the Idle phase after selection
        }

        // Right-click drag logic to start formation
        if (Input.GetMouseButtonDown(1) && !isSelecting) // Right-click to start dragging formation
        {
            startMousePosForFormation = Input.mousePosition;
            isFormationDrag = true; // Start the drag to define the formation
        }

        if (Input.GetMouseButton(1) && isFormationDrag) // While dragging right-click
        {
            float dragDistance = Vector2.Distance(startMousePosForFormation, Input.mousePosition);

            // Only start showing the formation preview if the drag distance exceeds the threshold
            if (dragDistance > dragThreshold)
            {
                int unitCount = context.SelectedObjects.Count;
                int lines = Mathf.Clamp(Mathf.FloorToInt(dragDistance / 20f) + 1, 1, 10);

                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var previewPositions = FormationCalculator.CalculateFormationPositions(mouseWorldPos, unitCount, Mathf.CeilToInt((float)unitCount / lines));

                context.previewer.ShowPreview(previewPositions);
            }
        }

        if (Input.GetMouseButtonUp(1) && isFormationDrag) // Right-click release after dragging
        {
            isFormationDrag = false;
            float dragDistance = Vector2.Distance(startMousePosForFormation, Input.mousePosition);
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (dragDistance <= dragThreshold)
            {
                // Quick right-click: use last remembered formation
                if (context.LastFormationOffsets.Count == context.SelectedObjects.Count)
                {
                    AssignOffsetPositions(mouseWorldPos, context.LastFormationOffsets);
                }
                else
                {
                    // Fallback to basic 3-line rectangle
                    int unitCount = context.SelectedObjects.Count;
                    int lines = 3;
                    var fallbackPositions = FormationCalculator.CalculateFormationPositions(mouseWorldPos, unitCount, lines);
                    context.LastFormationOffsets = FormationCalculator.CalculateOffsetsForUnits(unitCount, lines);
                    AssignUnitsToPositions(fallbackPositions);
                }
            }
            else
            {
                // Drag right-click: create new formation based on drag distance
                int unitCount = context.SelectedObjects.Count;
                int lines = Mathf.Clamp(Mathf.FloorToInt(dragDistance * 0.05f), 1, 10);
                var newPositions = FormationCalculator.CalculateFormationPositions(mouseWorldPos, unitCount, lines);
                context.LastFormationOffsets = FormationCalculator.CalculateOffsetsForUnits(unitCount, lines);
                AssignUnitsToPositions(newPositions);
            }

            context.previewer.Clear();
            context.SetPhase(new CommandPhase()); // Switch to CommandPhase to handle movement
        }
    }

    public void OnInitialMouseDown(Vector2 mousePos)
    {
        startPos = mousePos;
        isSelecting = true;
    }

    private IEnumerator SwitchToCommandPhase()
    {
        yield return null; // Wait one frame
        context.SetPhase(new CommandPhase());
    }

    private IEnumerator SwitchToIdle()
    {
        yield return null;
        context.SetPhase(new IdlePhase());
    }

    public void DrawBox()
    {
        if (!isSelecting) return;

        var rect = Utils.GetScreenRect(startPos, Input.mousePosition);
        Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
        Utils.DrawScreenRectBorder(rect, 2, Color.white);
    }

    private void TrySingleClickSelect()
    {
        Vector3 mouseWorldPos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorldPos = new Vector2(mouseWorldPos3D.x, mouseWorldPos3D.y);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, context.selectableLayer);

        if (hit == null)
        {
            DeselectAll();
            return;
        }

        Selectable selectable = hit.GetComponent<Selectable>();
        if (selectable == null)
        {
            DeselectAll(); // Only deselect if what we clicked was truly "nothing"
            return;
        }

        bool isMultiSelect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);

        if (!isMultiSelect)
        {
            DeselectAll();
            SelectObject(selectable);
        }
        else
        {
            if (context.SelectedObjects.Contains(selectable))
            {
                selectable.Deselect();
                context.SelectedObjects.Remove(selectable);
            }
            else
            {
                SelectObject(selectable);
            }
        }
    }

    private void DragSelect()
    {
        var viewportBounds = Utils.GetViewportBounds(Camera.main, startPos, Input.mousePosition);
        bool foundBuilding = false;

        List<Selectable> toSelect = new();

        foreach (var selectable in GameObject.FindObjectsByType<Selectable>(FindObjectsSortMode.None))
        {
            Vector3 viewPos = Camera.main.WorldToViewportPoint(selectable.transform.position);
            if (viewportBounds.Contains(viewPos))
            {
                toSelect.Add(selectable);
                if (selectable.building != null)
                {
                    foundBuilding = true;
                }
            }
        }

        if (toSelect.Count == 0)
        {
            return;
        }

        DeselectAll();

        if (foundBuilding)
        {
            foreach (var selectable in toSelect)
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
            foreach (var s in toSelect)
            {
                SelectObject(s);
            }
        }
    }

    private void SelectObject(Selectable selectable)
    {
        if (!context.SelectedObjects.Contains(selectable))
        {
            selectable.Select();
            context.SelectedObjects.Add(selectable);

            if (selectable.building != null)
            {
                context.StartCoroutine(ShowRadialMenuDelayed(selectable.building));
            }
        }
    }

    private void DeselectAll()
    {
        foreach (var s in context.SelectedObjects)
        {
            s.Deselect();
        }
        context.SelectedObjects.Clear();

        if (context.radialMenu != null)
        {
            context.radialMenu.Cancel();
        }
    }

    private IEnumerator ShowRadialMenuDelayed(Building building)
    {
        context.radialMenu.gameObject.SetActive(true);
        yield return null;

        if (building.buildingData?.spawnableUnits != null)
        {
            List<RadialMenuEntry> entries = new();
            foreach (var unit in building.buildingData.spawnableUnits)
            {
                if (unit == null) continue;
                var entry = ScriptableObject.CreateInstance<RadialMenuEntry>();
                entry.unitToSpawn = unit;
                entry.icon = unit.icon;
                entries.Add(entry);
            }

            RadialMenu.Instance.PopulateMenu(entries.ToArray(), building);
        }
    }

    private void AssignUnitsToPositions(List<Vector2> positions)
    {
        for (int i = 0; i < Mathf.Min(positions.Count, context.SelectedObjects.Count); i++)
        {
            context.SelectedObjects[i].MoveTo(positions[i]);
        }
    }

    private void AssignOffsetPositions(Vector2 center, List<Vector2> offsets)
    {
        for (int i = 0; i < Mathf.Min(offsets.Count, context.SelectedObjects.Count); i++)
        {
            Vector2 finalPos = center + offsets[i];
            context.SelectedObjects[i].MoveTo(finalPos);
        }
    }
}
