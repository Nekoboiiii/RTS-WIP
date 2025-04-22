using UnityEngine;
using System.Collections.Generic;

public class FormationPhase : ISelectionPhase
{
    private SelectionPhaseContext context;
    private Vector2 startMousePos;
    private bool isDragging = false;
    private float dragThreshold = 10f; // Pixel

    public void Enter(SelectionPhaseContext ctx)
    {
        Debug.Log($"[Phase] Entered: {GetType().Name}");
        context = ctx;
    }

    public void Exit()
    {
        Debug.Log($"[Phase] Exited: {GetType().Name}");
        isDragging = false;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            startMousePos = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButton(1) && isDragging)
        {
            float dragDistance = Vector2.Distance(startMousePos, Input.mousePosition);

            if (dragDistance > dragThreshold)
            {
                int unitCount = context.SelectedObjects.Count;
                int maxLines = 10;
                int lines = Mathf.Clamp(Mathf.FloorToInt(dragDistance / 20f) + 1, 1, maxLines);

                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Apply dynamic spacing here based on the drag distance
                float dynamicSpacing = Mathf.Clamp(dragDistance * 0.1f, 1.0f, 3.0f); // Adjust scaling factor

                var previewPositions = FormationCalculator.CalculateFormationPositions(mouseWorldPos, unitCount, Mathf.CeilToInt((float)unitCount / lines), dynamicSpacing);
                context.previewer.ShowPreview(previewPositions);
            }
        }

        if (Input.GetMouseButtonUp(1) && isDragging)
        {
            isDragging = false;

            float dragDistance = Vector2.Distance(startMousePos, Input.mousePosition);
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (dragDistance <= dragThreshold)
            {
                // Quick right-click: use the last remembered formation
                if (context.LastFormationOffsets.Count == context.SelectedObjects.Count)
                {
                    AssignOffsetPositions(mouseWorldPos, context.LastFormationOffsets);
                }
                else
                {
                    // Fallback to basic 3-line rectangle with dynamic spacing
                    int unitCount = context.SelectedObjects.Count;
                    int lines = 3;
                    float dynamicSpacing = Mathf.Clamp(dragDistance * 0.1f, 1.0f, 3.0f); // Adjust scaling factor

                    var fallback = FormationCalculator.CalculateFormationPositions(mouseWorldPos, unitCount, lines, dynamicSpacing);
                    int unitsPerLine = Mathf.CeilToInt((float)unitCount / lines);
                    context.LastFormationOffsets = FormationCalculator.CalculateOffsetsForUnits(unitCount, unitsPerLine, dynamicSpacing);

                    AssignUnitsToPositions(fallback);
                }
            }
            else
            {
                // Drag right-click: create new formation with dynamic spacing
                int unitCount = context.SelectedObjects.Count;
                int lines = Mathf.Max(1, Mathf.FloorToInt(dragDistance * 0.05f));
                float dynamicSpacing = Mathf.Clamp(dragDistance * 0.1f, 1.0f, 3.0f); // Adjust scaling factor

                var positions = FormationCalculator.CalculateFormationPositions(mouseWorldPos, unitCount, lines, dynamicSpacing);
                context.LastFormationOffsets = FormationCalculator.CalculateOffsetsForUnits(unitCount, lines, dynamicSpacing);
                AssignUnitsToPositions(positions);
            }

            context.previewer.Clear();
            context.SetPhase(new IdlePhase());
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
