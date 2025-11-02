using System;
using UnityEngine;
public class OnAir : IState
{
    private Entity _entity;
    private Action _combatMode;
    private int _actualLayer;
    public OnAir(Entity ent, Action _combatState)
    {
        _entity = ent;
        _combatMode=_combatState;
    }
    public void OnEnter()
    {
        _actualLayer = _entity.gameObject.layer;
        _entity.gameObject.layer = 17;
    }

    public void OnExit()
    {
       _entity.gameObject.layer = _actualLayer;
    }

    public void OnFixedUpdate()
    {
    
    }

    public void OnUpdate()
    {
      if(_entity.IsGrounded)
      {
            if (!_entity.Stuned)
            {
                _combatMode();
            }
      }
    }
}
