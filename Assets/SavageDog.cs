using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class SavageDog : Entity, Idamageable
{
    private Rigidbody _rb;
    public FsmSavageDog _fsm=new FsmSavageDog();
    private bool _isReady=false;
    [SerializeField]private Collider _collider;
    [Header("Debug")]
    public List<PathNode> _paths = new List<PathNode>();
    [Header("Variables")]
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private PhysicMaterial _movMat;
    [SerializeField] private PhysicMaterial _stopMat;
    public bool UseGravity=true;
    public bool CanMove = true;
    //[SerializeField] private float _velocity;
    #region Events
    public event Action<Vector3> OnMove = delegate { };
    #endregion
    private void Awake()
    {
        Kind = KindOfEntity.Enemy;
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
    }
    private void OnEnable()
    {
        //_fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        if (_isReady)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
        }
        GameManager.Instance.AddEntity(this, Kind);
    }
    private void Start()
    {
        //_fsm.AddState(FsmSavageDog.DogState.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat), OnMovePj, _rb));
    }
    private void Update()
    {
        //_fsm.ArtificialUpdate();
        //IsGroundedDetector();
    }
    private void FixedUpdate()
    {
        //_fsm.ArtificialFixedUpdate();
        IsGroundedDetector();
        if (UseGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce, 2), ForceMode.Acceleration);
        }
            Vector3 velocityChange = (Dir * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity) - new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.AddForce(velocityChange * 50, ForceMode.Acceleration);
        
        //_rb.MovePosition(transform.position + Dir * Time.fixedDeltaTime);
    }
    public void OnMovePj(Transform tg)
    {
        if (tg == null)
        {
            Dir = Vector3.zero;
            if (OnMove != null)
            {
                OnMove(Vector3.zero);
            }
            _collider.material=_stopMat;
            return;
        }
        if (CanMove)
        {
            _collider.material = _movMat;
        }
        AddForce(IaMov.Instance.Arrive(this, tg, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce));
        if (OnMove != null)
        {
            OnMove(Dir);
        }
    }
    private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0) { return; }
        Dir = Vector3.ClampMagnitude(Dir + target, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity);
    }
    private void OnDisable()
    {
        //_fsm.ChangeState(FsmMague.MagueStates.OnDeath);
        GameManager.Instance.RemoveEntity(this, Kind);
    }
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection)
    {
       
    }

    public void TakeHealt(float amount)
    {
       
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        PathNode ant = null;
        if (_paths.Count > 0) { Gizmos.DrawRay(transform.position, _paths[0].transform.position - transform.position); }
        foreach (PathNode i in _paths)
        {
            if (ant != null)
            {
                Gizmos.DrawRay(ant.transform.position, i.transform.position - ant.transform.position);
            }
            ant = i;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -Vector3.up * 10);
        if (_groundDetect.point != null)
        {
            if (Vector3.Distance(transform.position, _groundDetect.point) <= GroundDistanceDetector)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_groundDetect.point, 0.3f);
            }
        }
    }
}
