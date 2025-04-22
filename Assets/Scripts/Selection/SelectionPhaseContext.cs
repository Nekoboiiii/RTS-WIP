using System.Collections.Generic;
using UnityEngine;

public class SelectionPhaseContext : MonoBehaviour
{
    public ISelectionPhase CurrentPhase { get; private set; }
    public FormationPreviewer previewer;


    public List<Selectable> SelectedObjects = new();
    public List<Vector2> LastFormationOffsets = new();

    public LayerMask selectableLayer;
    public RadialMenu radialMenu;

    void Start()
    {
        SetPhase(new IdlePhase());
    }

    public void SetPhase(ISelectionPhase newPhase)
    {
        CurrentPhase?.Exit();
        CurrentPhase = newPhase;
        CurrentPhase?.Enter(this);
    }

    void Update()
    {
        CurrentPhase?.Update();
    }

    void OnGUI()
    {
        if (CurrentPhase is SelectingPhase selecting)
        {
            selecting.DrawBox();
        }
    }

    public void EnsureFormationOffsets()
    {
        int unitCount = SelectedObjects.Count;
        if (LastFormationOffsets == null || LastFormationOffsets.Count != unitCount)
        {
            int lines = 3;
            LastFormationOffsets = FormationCalculator.CalculateOffsetsForUnits(unitCount, lines);
        }
    }
}
