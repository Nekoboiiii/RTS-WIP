using UnityEngine;

public class IdlePhase : ISelectionPhase
{
    private SelectionPhaseContext context;

    public void Enter(SelectionPhaseContext ctx)
    {
        context = ctx;
    }

    public void Exit()
    {
        Debug.Log("[Phase] Exited: IdlePhase");
    }

    public void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 startClick = Input.mousePosition;

            // Immediately switch and *pass* the input
            SelectingPhase selecting = new SelectingPhase();
            context.SetPhase(selecting);
            selecting.OnInitialMouseDown(startClick); // New method for passing the input
        }

        if (Input.GetMouseButtonDown(1) && context.SelectedObjects.Count > 0)
        {
            Vector2 clickPos = Input.mousePosition;

            CommandPhase command = new CommandPhase();
            context.SetPhase(command);
            command.OnInitialRightClick(clickPos); // Pass along the intent
        }
    }
}
