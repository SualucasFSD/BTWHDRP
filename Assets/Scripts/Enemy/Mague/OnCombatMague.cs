using System.Collections.Generic;
using UnityEngine;

public class OnCombatMague : IState
{
    private FsmMague _fsm;
    private MagueEnemyModel _entity;
    private List<PathNode> _pathNodes = new List<PathNode>();
    private GameObject _tg;
    private float _magicTimer = 0;
    private int _acum;
    Quaternion _rotationVector;
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
        _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform);
    }

    public void OnExit()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
    }
    public void OnUpdate()
    {
        if(_entity.IsCharging&&_acum<3)
        {
            _magicTimer += Time.deltaTime;

            if (_tg != null)
            {
                RotateToFace(_tg.transform.position);
            }

            if (_magicTimer > 1.5f)
            {
                _entity.MagicInstance(_tg != null ? _tg.transform : null);
                _magicTimer = 0;
                _acum++;
            }

            if (_acum >= 3)
            {
                _acum = 0;
                _magicTimer = 0;
                _entity.Shoot();
            }

            return;
        }
        if(_tg!=null)
        {
            if (Vector3.Distance(_tg.transform.position, _entity.transform.position) < 5)
            {
                if (GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position))
                {
                    RotateToFace(_tg.transform.position);
                    _entity.OnMovePj(_tg.transform);
                    if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].AttackDistance)
                    {
                        _entity.StartCharging();
                        return;
                    }
                }
            }
            else
            {
                if (GameManager.Instance.SphereLineOfSight(_entity.transform.position, _tg.transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius))
                {
                    RotateToFace(_tg.transform.position);
                    _entity.OnMovePj(_tg.transform);
                    if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].AttackDistance)
                    {
                        _entity.StartCharging();
                        return;
                    }
                }
            }
        }

        if (_tg == null)
        {
            _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform);

            if (_tg == null)
            {
                _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
                return;
            }
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            PathReload();
        }

        _entity.OnMovePj(_pathNodes[0].transform);
        RotateToFace(_pathNodes[0].transform.position);

        if (Vector3.Distance(_pathNodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Height, _entity.transform.position) < 5f && GameManager.Instance.SphereLineOfSight(_entity.transform.position, _pathNodes[0].transform.position, 1f))
        {
            _pathNodes.RemoveAt(0);
        }
    }
    private void RotateToFace(Vector3 Target)
    {
        _rotationVector = Quaternion.Slerp(_entity.transform.rotation, Quaternion.LookRotation(Target - _entity.transform.position), GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce * 1.5f);
        _rotationVector.x = _entity.transform.rotation.x;
        _rotationVector.z = _entity.transform.rotation.z;
        _entity.transform.rotation = _rotationVector;
    }
    public void PathReload(params object[] objects)
    {
        _pathNodes = PathFinding.Instance.Theta(GameManager.Instance.GetCloseNode(_entity.transform), GameManager.Instance.GetCloseNode(GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform).transform), _entity.transform, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius);
        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
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

        if (totalDistance > 25f&&!_entity.IsCharging)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
            return;
        }
    }

    public void OnFixedUpdate()
    {
       
    }
}
