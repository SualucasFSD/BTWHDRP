using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class OnPatrol : IState
{
    private Entity _entity;
    private List<PathNode> _nodes = new List<PathNode>();
    private Action _combatState;
    private Action<Transform> _movePos;
    private LayerMask _nodesLayer;

    private float _timer = 0;
    private float _idleTime;
    private float _resetTimer = 0;

    private float _pathCooldown = 3f;
    private float _pathTimer = 0f;

    private Rigidbody _rb;
    private Vector3 _rotationDirection;
    private EnemyCatalogue _kind;

    private Transform _targetNode;

    public OnPatrol(Entity entity, LayerMask nodesLayer, Action combatState, Action<Transform> movePos, Rigidbody rb, EnemyCatalogue kind)
    {
        _entity = entity;
        _nodesLayer = nodesLayer;
        _combatState = combatState;
        _movePos = movePos;
        _rb = rb;
        _kind = kind;
    }

    public void OnEnter()
    {
        _idleTime = Random.Range(0, 15f);
        _nodes = _entity.TakePath(_entity.transform, _nodesLayer);
        _pathTimer = 0f;
    }

    public void OnExit()
    {
        _nodes.Clear();
        _targetNode = null;
        _rotationDirection = Vector3.zero;
    }

    public void OnUpdate()
    {
        _pathTimer += Time.deltaTime;

        if (_nodes.Count <= 0)
        {
            _timer += Time.deltaTime;
            _targetNode = null;

            if (_timer >= _idleTime && _pathTimer >= _pathCooldown)
            {
                _timer = 0f;
                _pathTimer = 0f;
                _idleTime = Random.Range(3, 20f);
                _nodes = _entity.TakePath(_entity.transform, _nodesLayer);
            }
        }

        if (_nodes.Count > 0)
        {
            var currentNodePos = _nodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[_kind].Height;
            float distanceToNode = Vector3.Distance(_entity.transform.position, currentNodePos);

            if (!GameManager.Instance.LineOfSight(_nodes[0].transform.position, _entity.transform.position))
            {
                _resetTimer += Time.deltaTime;
            }
            else
            {
                _resetTimer = 0f;
            }

            if (_resetTimer > 5f)
            {
                _idleTime = Random.Range(3, 20f);
                _nodes = _entity.TakePath(_entity.transform, _nodesLayer);
                _resetTimer = 0f;
            }

            Vector3 direction = (_nodes[0].transform.position - _entity.transform.position);
            _rotationDirection = direction.magnitude > 0.5f ? direction : Vector3.zero;

            if (distanceToNode <= 2f)
            {
                _nodes.RemoveAt(0);
            }

            _targetNode = (_nodes.Count > 0) ? _nodes[0].transform : null;
        }

        _entity.Detection(GameManager.Instance.EnemyConfiguration[_kind], _entity.transform, _combatState);
    }

    public void OnFixedUpdate()
    {
        /*if (_rotationDirection.sqrMagnitude > 0.001f)
        {
            Vector3 flatDir = new Vector3(_rotationDirection.x, 0f, _rotationDirection.z).normalized;

            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
                Quaternion deltaRot = targetRot * Quaternion.Inverse(_rb.rotation);

                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;

                if (Mathf.Abs(angle) > 1f)
                {
                    Vector3 torque = axis.normalized * angle * GameManager.Instance.EnemyConfiguration[_kind].RotForce;
                    _rb.AddTorque(torque, ForceMode.Acceleration);
                }
                else
                {
                    _rb.angularVelocity = Vector3.zero;
                }
            }
        }*/
        _movePos(_targetNode);
    }
}