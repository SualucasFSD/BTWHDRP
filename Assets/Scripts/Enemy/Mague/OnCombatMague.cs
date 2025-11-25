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
    private float _pathRetryTimer;

    public OnCombatMague(FsmMague fsm, MagueEnemyModel entity)
    {
        _fsm = fsm;
        _entity = entity;
    }

    public void OnEnter()
    {
        _magicTimer = 0f;
        _pathRetryTimer = 0f;

        _entity.BulletsStop();
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
        if (_entity.Life <= 0)
        {
            _entity.BulletsStop();
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
            return;
        }
        if (_entity == null) { return; }

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

        if (_entity.IsCharging && _entity.NumbOfBullets < 3 && !_entity.Stuned)
        {
            _magicTimer += Time.deltaTime;

            if (_tg != null)
            {
                _rotateDir = _tg.transform.position - _entity.transform.position;
            }
            if (_magicTimer > 1.5f)
            {
                _entity.MagicInstance(_tg.transform);
                _magicTimer = 0f;
            }

            if (_entity.NumbOfBullets >= 3)
            {
                _entity.Shoot();
            }
            _tg = null;
            return;
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            _pathRetryTimer += Time.deltaTime;

            if (_tg != null)
            {
                if (GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position))
                {
                    _rotateDir = _tg.transform.position - _entity.transform.position;

                    float dist = Vector3.Distance(_tg.transform.position, _entity.transform.position);
                    if (dist < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].AttackDistance &&
                        !_entity.IsCharging && !_entity.Stuned && _entity.IsGrounded)
                    {
                        _entity.StartCharging();
                    }
                }
            }

            if (_pathRetryTimer > 2f)
            {
                PathReload();
                _pathRetryTimer = 0f;
            }
            return;
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
            if (GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position) &&
                Vector3.Distance(_tg.transform.position, _entity.transform.position) <
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].AttackDistance)
            {
                if (!_entity.IsCharging && !_entity.Stuned && _entity.IsGrounded)
                    _entity.StartCharging();
            }
        }
    }

    public void OnFixedUpdate()
    {
        _entity.OnRotatePj(_rotateDir);
        _entity.OnMovePj();
    }

    private void PathReload(params object[] objects)
    {
        if (_entity == null) return;
        if (_tg == null) return;

        _pathNodes = PathFinding.Instance.Theta(
            GameManager.Instance.GetCloseNode(_entity.transform),
            GameManager.Instance.GetCloseNode(_tg.transform),
            _entity.transform,
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius);
    }
}

