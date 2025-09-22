using System.Collections.Generic;

public class FsmEnemyEsqueleton
{
    private IState _currenState;
    public enum AgentStates
    {
        OnPatrol,
        OnCombat,
        OnStunt,
        OnDeath,
        OnTakeDamage
    }
    Dictionary<AgentStates, IState> _states = new Dictionary<AgentStates, IState>();
    public void AddState(AgentStates newState, IState State)
    {
        if (!_states.ContainsKey(newState))
        {
            _states.Add(newState, State);
        }
    }
    public void ArtificialUpdate()
    {
        if (_currenState != null) { _currenState.OnUpdate(); }
    }
    public void ArtificialFixedUpdate()
    {
        if (_currenState != null) { _currenState.OnFixedUpdate(); }
    }
    public void ChangeState(AgentStates newState)
    {
        if (_currenState != null)
        {
            _currenState.OnExit();
        }
        _currenState = _states[newState];
        _currenState.OnEnter();
    }
}
