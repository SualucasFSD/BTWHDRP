using System;
using UnityEngine;
public class OnAir : IState
{
    //private FsmSavageDog _fsm;
    private Entity _entity;
    private Action _combatMode;
    private int _actualLayer;
    private float _iti = 0;
    public OnAir(Entity ent, Action _combatState)
    {
        _entity = ent;
        _combatMode=_combatState;
    }
    public void OnEnter()
    {
        _actualLayer = _entity.gameObject.layer;
        _entity.gameObject.layer = 17;
        _iti = 0;
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
      _iti += Time.deltaTime;
      if(_entity.IsGrounded&&_iti>1f)
      {
        _combatMode();
      }
    }
}
