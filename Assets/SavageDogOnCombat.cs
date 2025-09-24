using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavageDogOnCombat : IState
{
    private FsmEnemyEsqueleton _fsm;
    private SkeletonEnemyModel _entity;
    private List<PathNode> _pathNodes = new List<PathNode>();
    private Animator _anim;
    GameObject _tg;
    private Vector3 _dir = Vector3.zero;
    Quaternion _rotationVector;
    //Esquive
    private float _dogeProb = 100;
    private bool _isDodging = false;
    private float _dodgeTimer = 0f;
    public SavageDogOnCombat(FsmEnemyEsqueleton fsm, SkeletonEnemyModel entity, Animator anim)
    {
        _fsm = fsm;
        _entity = entity;
        _anim = anim;
    }
    public void OnEnter()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        EventManager.Suscribe(EventManager.KindOfEvent.PjAttack, DodgePosibilitie);
        PathReload();
        _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform);
    }

    public void OnExit()
    {
        _dir.z = 0f;
        _entity.OnMovePj(_dir);
        _anim.SetBool("isAttacking", false);
        EventManager.Unscribe(EventManager.KindOfEvent.ReloadPath, PathReload);
        EventManager.Unscribe(EventManager.KindOfEvent.PjAttack, DodgePosibilitie);
    }
    public void OnUpdate()
    {
        if (_isDodging)
        {
            _dir.z = 0f;

            _dodgeTimer -= Time.deltaTime;

            if (_dodgeTimer > 0f)
                return;
            _isDodging = false;
            _entity.IsDamageable = true;
            _dodgeTimer = 2;
        }

        if (_dodgeTimer > 0f)
        {
            _dodgeTimer -= Time.deltaTime;
        }

        if (_tg != null && _dodgeTimer <= 0f)
        {
            float distanceToTarget = Vector3.Distance(_tg.transform.position, _entity.transform.position);

            if (distanceToTarget < 2f && Vector3.Dot(_tg.transform.forward, (_entity.transform.position - _tg.transform.position).normalized) > 0.5f)
            {
                if (_dogeProb < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].DefenseProb)
                {
                    if (Physics.Raycast(_entity.transform.position + -_entity.transform.forward * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].DefenseDistance, -Vector3.up, 2f + GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Height))
                    {
                        _isDodging = true;
                        _entity.IsDodge = true;
                        _dodgeTimer = 1.5f;
                        _entity.IsDamageable = false;
                        _dir.z = 0f;
                        _entity.OnMovePj(_dir);
                        _anim.SetBool("isAttacking", false);
                        _anim.SetTrigger("Dodge");
                        return;
                    }
                }
            }
        }
        if (_tg != null)
        {
            if (Vector3.Distance(_tg.transform.position, _entity.transform.position) < 5)
            {
                if (GameManager.Instance.LineOfSight(_entity.transform.position, _tg.transform.position))
                {
                    RotateToFace(_tg.transform.position);

                    if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].AttackDistance)
                    {
                        _anim.SetBool("isAttacking", true);
                        _dir.z = 0f;
                    }
                    else
                    {
                        _anim.SetBool("isAttacking", false);
                        _dir.z = 0.2f;
                    }
                    _entity.OnMovePj(_dir);
                    return;
                }
            }
            else
            {
                if (GameManager.Instance.SphereLineOfSight(_entity.transform.position, _tg.transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Radius))
                {
                    RotateToFace(_tg.transform.position);

                    if (Vector3.Distance(_entity.transform.position, _tg.transform.position) < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].AttackDistance)
                    {
                        _anim.SetBool("isAttacking", true);
                        _dir.z = 0f;
                    }
                    else
                    {
                        _anim.SetBool("isAttacking", false);
                        _dir.z = 0.2f;
                    }
                    _entity.OnMovePj(_dir);
                    return;
                }
            }
            //return;
        }
        if (_tg == null)
        {
            _tg = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform);

            if (_tg == null)
            {
                _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
                return;
            }
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            PathReload();
        }

        _dir.z = 0.2f;
        _entity.OnMovePj(_dir);
        RotateToFace(_pathNodes[0].transform.position);

        if (Vector3.Distance(_pathNodes[0].transform.position + Vector3.up * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Height, _entity.transform.position) < 5f && GameManager.Instance.SphereLineOfSight(_entity.transform.position, _pathNodes[0].transform.position, 1f))
        {
            _pathNodes.RemoveAt(0);
        }
    }
    private void RotateToFace(Vector3 Target)
    {
        _rotationVector = Quaternion.Slerp(_entity.transform.rotation, Quaternion.LookRotation(Target - _entity.transform.position), GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].RotForce * 1.5f);
        _rotationVector.x = _entity.transform.rotation.x;
        _rotationVector.z = _entity.transform.rotation.z;
        _entity.transform.rotation = _rotationVector;
    }
    public void PathReload(params object[] objects)
    {
        _pathNodes = PathFinding.Instance.Theta(GameManager.Instance.GetCloseNode(_entity.transform), GameManager.Instance.GetCloseNode(GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(_entity.Kind), _entity.transform).transform), _entity.transform, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Radius);
        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
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
            _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
            return;
        }
    }
    public void DodgePosibilitie(object[] objects)
    {
        _dogeProb = Random.Range(1, 101);
    }

    public void OnFixedUpdate()
    {

    }
}
