using System.Collections.Generic;
using UnityEngine;

public class FsmSavageDog : MonoBehaviour
{
    private IState _currenState;
    public enum DogState
    {
        OnPatrol,
        OnPursuit,
        OnCombat,
        OnStunt,
        OnDeath
    }
    Dictionary<DogState, IState> _states = new Dictionary<DogState, IState>();
    public void AddState(DogState newState, IState State)
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
    public void ChangeState(DogState newState)
    {
        if (_currenState != null)
        {
            _currenState.OnExit();
        }
        _currenState = _states[newState];
        _currenState.OnEnter();
    }
}
