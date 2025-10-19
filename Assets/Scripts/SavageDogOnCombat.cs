using System.Collections.Generic;
using UnityEngine;

public class SavageDogOnCombat : IState
{
    private FsmSavageDog _fsm;
    private SavageDog _entity;
    private List<PathNode> _pathNodes = new List<PathNode>();
    private GameObject _tg;
    private Vector3 _rotateDir;

    public SavageDogOnCombat(FsmSavageDog fsm, SavageDog entity)
    {
        _fsm = fsm;
        _entity = entity;
    }

    public void OnEnter()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind),_entity.transform);
    }

    public void OnExit()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        _pathNodes.Clear();
        _entity.Tg = null;
    }

    public void OnUpdate()
    {
        if (_entity.Life <= 0)
        {
            return;
        }
        if (_tg == null)
        {
            _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind),_entity.transform);

            if (_tg == null)
            {
                _entity.Tg = _entity.transform;
                _rotateDir = Vector3.zero;
                _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
                return;
            }
        }

        float distToTarget = Vector3.Distance(_tg.transform.position, _entity.transform.position);

        if (distToTarget < 5f)
        {
            if (GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position))
            {
                HandleCombat();
                return;
            }
        }
        else
        {
            if (GameManager.Instance.SphereLineOfSight(_entity.transform.position,_tg.transform.position,GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius))
            {
                HandleCombat();
                return;
            }
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            PathReload();

            if (_pathNodes == null || _pathNodes.Count == 0)
            {
                _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
                return;
            }
        }

        if (_pathNodes.Count > 0)
        {
            _entity.Tg = _pathNodes[0].transform;
            _rotateDir = _pathNodes[0].transform.position - _entity.transform.position;

            Vector3 nodePos = _pathNodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Height;
            if (Vector3.Distance(nodePos, _entity.transform.position) < 5f &&
                GameManager.Instance.SphereLineOfSight(_entity.transform.position, _pathNodes[0].transform.position, 1f))
            {
                _pathNodes.RemoveAt(0);
            }
        }
    }

    private void HandleCombat()
    {
        _entity.Tg = _tg.transform;
        _rotateDir = _tg.transform.position - _entity.transform.position;

        if (!_entity.Stuned && _entity.CanAttack)
        {
            float attackDist = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].AttackDistance;
            if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < attackDist)
            {
                _entity.Attack();
            }
        }
    }

    public void PathReload(params object[] objects)
    {
        if (_tg == null)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            return;
        }

        _pathNodes = PathFinding.Instance.Theta(GameManager.Instance.GetCloseNode(_entity.transform),GameManager.Instance.GetCloseNode(_tg.transform),_entity.transform,GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius);

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            return;
        }

        float totalDistance = 0f;
        PathNode previous = null;
        foreach (PathNode node in _pathNodes)
        {
            totalDistance += (previous == null)? Vector3.Distance(_entity.transform.position, node.transform.position): Vector3.Distance(previous.transform.position, node.transform.position);
            previous = node;
        }

        /*if (totalDistance > 30f)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            _pathNodes.Clear();
            return;
        }*/
    }

    public void OnFixedUpdate()
    {
        if (_entity.Tg == null) return;

        _entity.OnRotatePj(_rotateDir);
        _entity.OnMovePj();
    }
}
