using System.Collections.Generic;

public class FsmDemonBoss
{
    private IState _currenState;
    public enum AgentStates
    { 
        OnMidAir,
        OnCombat,
        OnStunt,
        OnDeath,
        OnGoinAir,
        OnGoinGround,
        ChooseState
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

