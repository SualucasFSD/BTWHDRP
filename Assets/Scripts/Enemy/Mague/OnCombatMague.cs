using System.Collections.Generic;
using UnityEngine;

public class OnCombatMague : IState
{
    private readonly FsmMague _fsm;
    private readonly MagueEnemyModel _entity;

    private List<PathNode> _pathNodes = new List<PathNode>();
    private GameObject _tg;
    private Vector3 _rotateDir;

    private float _magicTimer;
    private int _acum;

    public OnCombatMague(FsmMague fsm, MagueEnemyModel entity)
    {
        _fsm = fsm;
        _entity = entity;
    }

    public void OnEnter()
    {
        _magicTimer = 0;
        _acum = 0;

        EventManager.Suscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        PathReload();

        _tg = GameManager.Instance.GetCloseEnemy(
            GameManager.Instance.RefreshEnemy(_entity.Kind),
            _entity.transform);
    }

    public void OnExit()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        _pathNodes.Clear();
        _rotateDir = Vector3.zero;
    }

    public void OnUpdate()
    {
        if (_entity.IsCharging && _entity.NumbOfBullets < 3)
        {
            _magicTimer += Time.deltaTime;

            if (_tg != null)
            {
                _rotateDir = _tg.transform.position - _entity.transform.position;
            }
            if (_magicTimer > 1.5f)
            {
                _entity.MagicInstance(_tg ? _tg.transform : null);
                _magicTimer = 0;
                _acum++;
            }

            if (_acum >= 3)
            {
                _acum = 0;
                _entity.Shoot();
            }

            return;
        }

        if (_tg == null)
        {
            _tg = GameManager.Instance.GetCloseEnemy(
                GameManager.Instance.RefreshEnemy(_entity.Kind),
                _entity.transform);

            if (_tg == null)
            {
                _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
                return;
            }
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            PathReload();
            if (_pathNodes == null || _pathNodes.Count == 0)
            {
                _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
                return;
            }
        }

        PathNode currentNode = _pathNodes[0];
        _entity.Tg = currentNode.transform;
        _rotateDir = currentNode.transform.position - _entity.transform.position;

        float distanceToNode = Vector3.Distance(
            currentNode.transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Height,
            _entity.transform.position);

        if (distanceToNode < 5f &&
            GameManager.Instance.SphereLineOfSight(_entity.transform.position, currentNode.transform.position, 1f))
        {
            _pathNodes.RemoveAt(0);
        }

        if (_tg != null)
        {
            bool inRange = Vector3.Distance(_tg.transform.position, _entity.transform.position) <
                           GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].AttackDistance;

            bool hasLOS = GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position);

            if (hasLOS && inRange)
            {
                HandleCombat();
            }
        }
    }

    private void HandleCombat()
    {
        if (_tg == null) return;

        _rotateDir = _tg.transform.position - _entity.transform.position;
        _entity.Tg = _tg.transform;

        if (Vector3.Distance(_entity.transform.position, _tg.transform.position) <
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].AttackDistance)
        {
            _entity.StartCharging();
        }
    }

    private void PathReload(params object[] objects)
    {
        if (_entity == null) return;

        if (_tg == null)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
            return;
        }

        _pathNodes = PathFinding.Instance.Theta(GameManager.Instance.GetCloseNode(_entity.transform),GameManager.Instance.GetCloseNode(_tg.transform),_entity.transform,GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius);

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
            return;
        }

        float totalDistance = 0f;
        PathNode previous = null;
        foreach (PathNode node in _pathNodes)
        {
            totalDistance += (previous == null)? Vector3.Distance(_entity.transform.position, node.transform.position): Vector3.Distance(previous.transform.position, node.transform.position);
            previous = node;
        }

        if (totalDistance > 30)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
            _pathNodes.Clear();
        }
    }

    public void OnFixedUpdate()
    {
        if (_entity == null || _entity.Tg == null) return;

        _entity.OnRotatePj(_rotateDir);
        _entity.OnMovePj();
    }
}
