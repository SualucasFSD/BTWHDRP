using System;
using UnityEngine;

public class ToTheGroundState : IState
{
    Rigidbody _rb;
    Entity _ent;
    float _force;
    EnemyCatalogue _kind;
    Action _combatMode;
    public ToTheGroundState(Entity Ent,Rigidbody Rb,float Force,EnemyCatalogue Kind,Action CombatMode)
    {
        _ent = Ent;
        _rb = Rb;
        _force = Force;
        _kind = Kind;
        _combatMode = CombatMode;
    }
    public void OnEnter()
    {
        _ent.Stuned = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _ent.GravValue = GameManager.Instance.EnemyConfiguration[_kind].GravityForce;
        _ent.UseGravity = true;

        _rb.AddForce(-Vector3.up * _force, ForceMode.Impulse);
    }

    public void OnExit()
    {
       
    }

    public void OnFixedUpdate()
    {
       
    }

    public void OnUpdate()
    {
        if (_ent.IsGrounded)
        {
            if (!_ent.Stuned)
            {
               _combatMode();
            }
        }
    }
}
