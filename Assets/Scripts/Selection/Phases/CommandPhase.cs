using UnityEngine;

public class CommandPhase : ISelectionPhase
{
    private SelectionPhaseContext context;
    private float moveCommandTimer;
    private float moveCommandInterval = 0.1f;

    private Vector2? queuedInitialTarget = null;

    public void Enter(SelectionPhaseContext ctx)
    {
        Debug.Log($"[Phase] Entered: {GetType().Name}");
        context = ctx;
        moveCommandTimer = moveCommandInterval;

        // If there's a pending initial move command, process it
        if (queuedInitialTarget.HasValue)
        {
            IssueMoveCommand(queuedInitialTarget.Value);
            queuedInitialTarget = null;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButton(1)) // Holding right-click to issue repeat movement
        {
            moveCommandTimer += Time.deltaTime;
            if (moveCommandTimer >= moveCommandInterval)
            {
                moveCommandTimer = 0f;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                IssueMoveCommand(worldPos);
            }
        }

        if (Input.GetMouseButtonDown(0)) // Left-click to go back to selection
        {
            context.SetPhase(new SelectingPhase());
        }
    }

    public void Exit()
    {
        Debug.Log($"[Phase] Exited: {GetType().Name}");
        moveCommandTimer = moveCommandInterval;
    }

    public void OnInitialRightClick(Vector2 screenMousePos)
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(screenMousePos);
        Vector2 worldPos2D = new Vector2(world.x, world.y);
        
        // If context is already assigned (phase is active), move immediately
        if (context != null)
        {
            IssueMoveCommand(worldPos2D);
        }
        else
        {
            // Phase not yet fully initialized—queue the move for later
            queuedInitialTarget = worldPos2D;
        }
    }

    private void IssueMoveCommand(Vector2 targetCenter)
    {
        var offsets = context.LastFormationOffsets;

        // Ensure valid offsets
        if (offsets == null || offsets.Count != context.SelectedObjects.Count)
        {
            int unitCount = context.SelectedObjects.Count;
            int estimatedLines = Mathf.Clamp(Mathf.FloorToInt(Mathf.Sqrt(unitCount)), 1, 10); // Estimate a good number of lines based on unit count
            
            // Apply dynamic spacing (you can adjust this logic further to pass in the correct spacing value if necessary)
            float dynamicSpacing = 1.5f; // Adjust if needed, or pass it dynamically
            offsets = FormationCalculator.CalculateOffsetsForUnits(unitCount, estimatedLines, dynamicSpacing);
            context.LastFormationOffsets = offsets;
        }

        int i = 0;
        foreach (var selectable in context.SelectedObjects)
        {
            if (selectable.unit == null) continue;

            Vector2 offset = (i < offsets.Count) ? offsets[i] : Vector2.zero;
            selectable.unit.MoveTo(targetCenter + offset);
            i++;
        }
    }
}
