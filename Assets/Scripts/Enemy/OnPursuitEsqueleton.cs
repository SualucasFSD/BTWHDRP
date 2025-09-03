using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPursuitEsqueleton : IState
{
    FsmEnemyEsqueleton _fsm;
    SkeletonEnemyModel _entity;
    public OnPursuitEsqueleton(FsmEnemyEsqueleton fsm, SkeletonEnemyModel entity)
    {
        _fsm = fsm;
        _entity = entity;
    }

    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
