public interface ISelectionPhase
{
    void Enter(SelectionPhaseContext context);
    void Update();
    void Exit();
}
