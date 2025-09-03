using System.Collections.Generic;

public class FsmMague
{
    private IState _currenState;
    public enum MagueStates
    {
        OnPatrol,
        OnPursuit,
        OnCombat,
        OnStunt,
        OnDeath
    }
    Dictionary<MagueStates, IState> _states = new Dictionary<MagueStates, IState>();
    public void AddState(MagueStates newState, IState State)
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
    public void ChangeState(MagueStates newState)
    {
        if (_currenState != null)
        {
            _currenState.OnExit();
        }
        _currenState = _states[newState];
        _currenState.OnEnter();
    }
}
