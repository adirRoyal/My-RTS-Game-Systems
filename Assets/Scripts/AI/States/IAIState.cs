/// <summary>
/// Interface for AI States (used in a State Machine pattern)
/// Any AI state (Idle, Patrol, Chasing, Gathering, etc.) must implement this.
/// </summary>
public interface IAIState
{
    /// <summary>
    /// Called once when entering the state.
    /// Use this to initialize variables, set animations, or start movement.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called every frame while the AI is in this state.
    /// Use this to handle logic like moving, detecting enemies, or performing actions.
    /// </summary>
    void Update();

    /// <summary>
    /// Called once when exiting the state.
    /// Use this to clean up, stop animations, or cancel actions.
    /// </summary>
    void Exit();
}
