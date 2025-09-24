using System.Collections.Generic;
using UnityEngine;

public class SavageDogOnCombat : IState
{
    private FsmSavageDog _fsm;
    private SavageDog _entity;
    private List<PathNode> _pathNodes = new List<PathNode>();
    GameObject _tg;
    private Vector3 _rotateDir;
    private Transform _fallowTg;
    public SavageDogOnCombat(FsmSavageDog fsm, SavageDog entity)
    {
        _fsm = fsm;
        _entity = entity;
    }
    public void OnEnter()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        PathReload();
        _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform);
    }

    public void OnExit()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
    }
    public void OnUpdate()
    {
        if (_tg != null)
        {
            _rotateDir = _tg.transform.position - _entity.transform.position;
            if (Vector3.Distance(_tg.transform.position, _entity.transform.position) < 3)
            {
                if (GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position))
                {
                    if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].AttackDistance&&!_entity.Stuned)
                    {
                        _entity.Attack();
                    }
                    _fallowTg = _tg.transform;
                    return;
                }
            }
            else
            {
                if (GameManager.Instance.SphereLineOfSight(_entity.transform.position, _tg.transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius))
                {
                    if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].AttackDistance && !_entity.Stuned)
                    {
                        _entity.Attack();
                    }
                    _fallowTg = _tg.transform;
                    return;
                }
            }
        }
        else
        {
            _fallowTg=null;
        }
        if (_tg == null)
        {
            _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform);

            if (_tg == null)
            {
                _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
                return;
            }
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            PathReload();
        }

        _fallowTg = _pathNodes[0].transform;
        _rotateDir = _pathNodes[0].transform.position - _entity.transform.position;
        if (Vector3.Distance(_pathNodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Height, _entity.transform.position) < 5f && GameManager.Instance.SphereLineOfSight(_entity.transform.position, _pathNodes[0].transform.position, 1f))
        {
            _pathNodes.RemoveAt(0);
        }
    }
    public void PathReload(params object[] objects)
    {
        _pathNodes = PathFinding.Instance.Theta(GameManager.Instance.GetCloseNode(_entity.transform), GameManager.Instance.GetCloseNode(GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform).transform), _entity.transform, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius);
        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            return;
        }
        float totalDistance = 0f;
        PathNode previous = null;
        foreach (PathNode node in _pathNodes)
        {
            totalDistance += (previous == null)
                ? Vector3.Distance(_entity.transform.position, node.transform.position)
                : Vector3.Distance(previous.transform.position, node.transform.position);

            previous = node;
        }

        if (totalDistance > 25f)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            return;
        }
    }
    public void OnFixedUpdate()
    {
        if (_rotateDir.sqrMagnitude > 0.01f)
        {
          _entity.OnRotatePj(_rotateDir);
        }
        if (_fallowTg != null)
        {
            _entity.OnMovePj(_fallowTg);
        }
    }
}
