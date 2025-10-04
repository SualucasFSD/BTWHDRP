using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class OnPatrol : IState
{
    private readonly Entity _entity;
    private readonly Action _combatState;
    private readonly Action _movePos;
    private readonly Action<Vector3> _rotatePos;
    private readonly LayerMask _nodesLayer;
    private readonly Rigidbody _rb;
    private readonly EnemyCatalogue _kind;

    private List<PathNode> _nodes = new List<PathNode>();
    private Vector3 _rotationDirection = Vector3.zero;

    private float _idleTime;
    private float _idleTimer;
    private float _pathCooldown = 3f;
    private float _pathTimer;
    private float _resetTimer;

    public OnPatrol(Entity entity, LayerMask nodesLayer, Action combatState, Action movePos, Action<Vector3> rotatePos, Rigidbody rb, EnemyCatalogue kind)
    {
        _entity = entity;
        _nodesLayer = nodesLayer;
        _combatState = combatState;
        _movePos = movePos;
        _rotatePos = rotatePos;
        _rb = rb;
        _kind = kind;
    }

    public void OnEnter()
    {
        _idleTime = Random.Range(2f, 10f);
        _idleTimer = 0f;
        _pathTimer = 0f;
        _resetTimer = 0f;

        RequestNewPath();
    }

    public void OnExit()
    {
        _nodes.Clear();
        _rotationDirection = Vector3.zero;
        _entity.Tg = null;
    }

    public void OnUpdate()
    {
        _pathTimer += Time.deltaTime;

        if (_nodes == null || _nodes.Count == 0)
        {
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= _idleTime && _pathTimer >= _pathCooldown)
            {
                _idleTimer = 0f;
                _pathTimer = 0f;
                _idleTime = Random.Range(3f, 15f);
                RequestNewPath();
            }

            _entity.Detection(GameManager.Instance.EnemyConfiguration[_kind], _entity.transform, _combatState);
            return;
        }

        Vector3 nodePos = _nodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[_kind].Height;
        float distanceToNode = Vector3.Distance(_entity.transform.position, nodePos);

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
            RequestNewPath();
            _resetTimer = 0f;
            return;
        }

        Vector3 direction = _nodes[0].transform.position - _entity.transform.position;
        _rotationDirection = direction.sqrMagnitude > 0.25f ? direction : Vector3.zero;

        if (distanceToNode <= 2f)
        {
            _nodes.RemoveAt(0);
            if (_nodes.Count == 0)
            {
                _entity.Tg = _entity.transform;
                _rotationDirection = Vector3.zero;
            }
        }
        else
        {
            _entity.Tg = _nodes[0].transform;
        }

        _entity.Detection(GameManager.Instance.EnemyConfiguration[_kind], _entity.transform, _combatState);
    }

    public void OnFixedUpdate()
    {
        if (_entity == null) return;

        _rotatePos(_rotationDirection);
        _movePos();
    }

    private void RequestNewPath()
    {
        _nodes = _entity.TakePath(_entity.transform, _nodesLayer);

        if (_nodes == null || _nodes.Count == 0)
        {
            _idleTime = Random.Range(3f, 15f);
            _idleTimer = 0f;
            return;
        }

        if (Vector3.Distance(_entity.transform.position, _nodes[0].transform.position) < 1.5f)
        {
            _nodes.RemoveAt(0);
        }

        _entity.Tg = (_nodes.Count > 0) ? _nodes[0].transform : _entity.transform;
    }
}
