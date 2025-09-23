using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;
using Unity.Mathematics;
[RequireComponent(typeof(Rigidbody))]
public class SavageDog : Entity, Idamageable
{
    private Rigidbody _rb;
    private bool _stuned = false;
    private float _actualAirTime;
    private bool _inAirCombo=false;
    private float _groundDelay=0;
    private Coroutine _floatRoutine;
    public FsmSavageDog _fsm=new FsmSavageDog();
    private bool _isReady=false;
    [SerializeField]private Collider _collider;
    [Header("Debug")]
    public List<PathNode> _paths = new List<PathNode>();
    [Header("Variables")]
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private PhysicMaterial _movMat;
    [SerializeField] private PhysicMaterial _stopMat;
    [SerializeField] private float _ceilingOffset;
    [SerializeField] private float _groundImpulse=2500f;
    public bool UseGravity=true;
    public bool CanMove = true;
    //[SerializeField] private float _velocity;
    #region Events
    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAirHit=delegate { };      
    public event Action OnHitStunt= delegate { };
    public event Action OnFreeFall= delegate { };
    public event Action<bool> OnGrounded= delegate { };
    #endregion
    private void Awake()
    {
      /*  Kind = KindOfEntity.Enemy;
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }*/
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
        //_fsm.AddState(FsmSavageDog.DogState.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat), OnMovePj, _rb));
    }
    private void Update()
    {
        _groundDelay += Time.deltaTime;
        if (_groundDelay > 1.5f)
        {
            OnGrounded(IsGrounded);
        }
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
        if (Dir == Vector3.zero)
        {
            _rb.angularVelocity = Vector3.zero;
            return;
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
        if (IsGrounded&&!_inAirCombo)
        {
            OnHitStunt();
        }
        else
        {
            MantainOnAir();
        }

    }

    public void TakeHealt(float amount)
    {
       
    }
    public override void FlyFunct(float height = 4)
    {
        _stuned = true;
        _actualAirTime = 0;
        //_fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);

        UseGravity = false;
        if (!_rb.isKinematic)
            _rb.velocity = Vector3.zero;

        float targetY = transform.position.y + height;

        if (Physics.SphereCast(transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius, Vector3.up, out RaycastHit hit, height+0.5f, layerMask: GroundLayer))
        {
            targetY = hit.point.y - _ceilingOffset;
        }

        //gameObject.layer = _airLayer;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        _floatRoutine=StartCoroutine(GoUpAndFloat(targetY));
    }
    private void MantainOnAir()
    {
        _actualAirTime = 0;
    }
    private IEnumerator GoUpAndFloat(float targetY)
    {
        while (transform.position.y < targetY)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            if (!_rb.isKinematic)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, 50f * Time.deltaTime);
                _rb.MovePosition(pos);
            }
            yield return null;
        }
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
        while (_actualAirTime < 2.5f - 0.5f)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            if (!_rb.isKinematic)
            {
                _rb.MovePosition(transform.position);
            }
            _actualAirTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        _groundDelay = 0;
        OnFreeFall();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX| RigidbodyConstraints.FreezeRotationZ;
        _stuned = false;
        UseGravity = true;
    }
    public override void GetToTheGround()
    {
      if (_floatRoutine!=null)
      {
         StopCoroutine(_floatRoutine);
         _floatRoutine = null;
      }
        OnFreeFall();
        _groundDelay = 0;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.AddForce(-Vector3.up * _groundImpulse, ForceMode.Impulse);
        _stuned = false;
        UseGravity = true;
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
