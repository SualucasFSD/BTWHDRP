using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
public class OnPatrol : IState
{
    private Entity _entity;
    private List<PathNode> _nodes = new List<PathNode>();
    Action _combatState;
    Action<Transform> _movePos;
    private LayerMask _nodesLayer;
    private float _timer = 0;
    private float _idleTime;
    private float _resetTimer = 0;
    private Quaternion _rotationVector;
    public OnPatrol(Entity Entity,LayerMask NodesLayer, Action CombatState,Action<Transform> MovePos)
    {
        _nodesLayer = NodesLayer;
        _entity = Entity;
        _combatState = CombatState;
        _movePos = MovePos;
    }
    public void OnEnter()
    {
        _idleTime = Random.Range(0, 15f);
        _nodes = _entity.TakePath(_entity.transform, _nodesLayer);
    }

    public void OnExit()
    {
        _nodes = new List<PathNode>();
    }

    public void OnUpdate()
    {
        if (_nodes.Count <= 0 && _timer >= _idleTime)
        {
            _timer = 0f;
            _idleTime = Random.Range(3, 20f);
            _nodes = _entity.TakePath(_entity.transform, _nodesLayer);
        }

        if (_nodes.Count > 0)
        {
            var currentNodePos = _nodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Height;
            float distanceToNode = Vector3.Distance(_entity.transform.position, currentNodePos);

            if (!GameManager.Instance.LineOfSight(_nodes[0].transform.position, _entity.transform.position))
                _resetTimer += Time.deltaTime;
            else
                _resetTimer = 0f;

            if (_resetTimer > 5f)
            {
                _idleTime = Random.Range(3, 20f);
                _nodes = _entity.TakePath(_entity.transform, _nodesLayer);
                return;
            }

            Vector3 direction = (_nodes[0].transform.position - _entity.transform.position);
            if (direction.magnitude > 0.5f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
                _rotationVector = Quaternion.Slerp(_entity.transform.rotation, targetRot, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce);
                _rotationVector.x = _entity.transform.rotation.x;
                _rotationVector.z = _entity.transform.rotation.z;
                _entity.transform.rotation = _rotationVector;
            }

            if (distanceToNode <= 2f)
            {
                _nodes.RemoveAt(0);
            }
            if (_nodes.Count > 0)
            {
                _movePos(_nodes[0].transform);
            }
        }
        else
        {
            _timer += Time.deltaTime;
            _movePos(null);
        }
        _entity.Detection(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague], _entity.transform, _combatState);
    }
}
