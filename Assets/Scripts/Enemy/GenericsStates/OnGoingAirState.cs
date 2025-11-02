using System;
using UnityEngine;

public class OnGoingAirState : IState
{
    Entity _ent;
    Rigidbody _rb;
    float _distance;
    float _ceilingOffset;
    float _maxTimer;
    Action _onAirState;
    private float _targetY;
    public OnGoingAirState(Entity Ent,Action OnAirState,float Distance,float CeilingOffset, Rigidbody Rb)
    {
        _ent = Ent;
        _rb = Rb;
        _distance = Distance;
        _ceilingOffset = CeilingOffset;
        _onAirState = OnAirState;
    }
    public void OnEnter()
    {
        _ent.Stuned = true;
        _maxTimer = 0;
        _ent.UseGravity = false;
        _ent.GravValue = 0;
        if(!_rb.isKinematic)
        {
            _rb.angularVelocity = Vector3.zero;
            _rb.linearVelocity = Vector3.zero;
        }
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        _targetY = _ent.transform.position.y + _distance;
        if (Physics.SphereCast(_ent.transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius, Vector3.up, out RaycastHit hit,_distance + 0.5f, layerMask: _ent.GroundLayer))
        {
            _targetY = hit.point.y - _ceilingOffset;
        }
    }

    public void OnExit()
    {
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _ent.UseGravity = true;
    }

    public void OnFixedUpdate()
    {
        if (_ent.transform.position.y < _targetY&&_maxTimer<=1.5f)
        {
            //Debug.Log("Subiendo");
            _rb.MovePosition(_rb.position + Vector3.up * 20f*Time.fixedDeltaTime);
            return;
        }
        _onAirState();
    }

    public void OnUpdate()
    {
        _maxTimer += Time.deltaTime;
    }
}
