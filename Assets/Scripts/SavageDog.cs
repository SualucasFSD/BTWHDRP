using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;
[RequireComponent(typeof(Rigidbody))]
public class SavageDog : Entity, Idamageable
{
    private Rigidbody _rb;
    public bool Stuned = false;
    private Coroutine _floatRoutine;
    public FsmSavageDog _fsm=new FsmSavageDog();
    private bool _isReady=false;
    [SerializeField]private Collider _collider;
    [Header("Debug")]
    public List<PathNode> _paths = new List<PathNode>();
    [Header("Variables")]
    [SerializeField] private ParticleSystem _bloodVfx;
    [SerializeField] private LifeOrb _lifeOrbPrefab;
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private float _ceilingOffset;
    [SerializeField] private float _groundImpulse=2500f;
    [SerializeField] private float _attackDelay=2.5f;
    public bool UseGravity=true;
    public bool CanMove = true;
    private float _gravityValue;
    private float _attackTimer;
    public bool CanAttack=true;
    private bool _isDead=false;
    #region Events
    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAttack = delegate { };
    public event Action OnAirHit=delegate { };      
    public event Action OnHitStunt= delegate { };
    public event Action GetToAir = delegate { };
    public event Action GetToGround = delegate { };
    public event Action OnFreeFall= delegate { };
    public event Action<bool> OnGrounded= delegate { };
    #endregion
    private void Awake()
    {
       // Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;
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
    public override void EnableAgain()
    {
        _attackTimer = _attackDelay;
        StartCoroutine(DelayedInit());
    }
    private IEnumerator DelayedInit()
    {
        yield return null;
        if (_isReady)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;
        }
        GameManager.Instance.AddEntity(this, Kind);
    }
    private void Start()
    {
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;
        _fsm.AddState(FsmSavageDog.DogState.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat), OnMovePj,OnRotatePj,EnemyCatalogue.SavageDog));
        _fsm.AddState(FsmSavageDog.DogState.OnCombat, new SavageDogOnCombat(_fsm,this));
        _fsm.AddState(FsmSavageDog.DogState.OnMidAir, new OnAir(this, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat)));
        _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
        _isReady = true;
    }
    private void Update()
    {
        if (_isDead)
        {
            return;
        }
        if (_attackTimer<_attackDelay)
        {
            _attackTimer += Time.deltaTime;
        }
        if(_gravityValue< GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce)
        {
            _gravityValue += Time.deltaTime*7f;
        }
        OnGrounded(IsGrounded);
        _fsm.ArtificialUpdate();
        if (Tg != null)
        {
          AddForce(IaMov.Instance.Separation(GameManager.Instance.GetSeparationEntityes(), 1.8f, this, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce)+IaMov.Instance.Arrive(this, Tg, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce) * 0.8f+IaMov.Instance.ObstacleAvoid(this, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce,GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius));
        }
    }
    private void FixedUpdate()
    {
        if (_isDead)
        {
            return;
        }
        IsGroundedDetector();
        if (UseGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(_gravityValue, 2), ForceMode.Acceleration);
        }
        if (!Stuned)
        {
            _fsm.ArtificialFixedUpdate();
        }
        if (Dir == Vector3.zero)
        {
            _rb.angularVelocity = Vector3.zero;
            return;
        }
    }
    public void OnMovePj()
    {
        if (Tg == null||_attackTimer<_attackDelay||!CanAttack)
        {
            Dir = Vector3.zero;
            if (OnMove != null)
            {
                OnMove(Vector3.zero);
            }
            return;
        }
        if (OnMove != null)
        {
            OnMove(Dir);
        }
        Vector3 velocityChange = (new Vector3(Dir.x,0,Dir.z) * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity) - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        _rb.AddForce(velocityChange, ForceMode.Acceleration);
    }
    public void OnRotatePj(Vector3 Direction)
    {
        if (_rb == null) return;
        if (Direction.sqrMagnitude <= 0.001f && !CanAttack) return;

        Direction.y = 0f;
        if (Direction.sqrMagnitude < 0.0001f) return;

        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(Direction.normalized, Vector3.up), GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce * Time.fixedDeltaTime));
    }
    private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0) { Dir = Vector3.zero; return; }
        Dir = Vector3.ClampMagnitude(Dir + target, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity);
    }
    private void InvokeCanAttack()
    {
        CanAttack = true;
    }
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false)
    {
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        if (Life <= 0)
            return;

        CanAttack = false;

        if (_bloodVfx != null)
        {
            _bloodVfx.Play();
        }
        Invoke(nameof(InvokeCanAttack), 1.5f);
        Life -= dmg;

        if (!_isDead)
        {
            if (!IsGrounded && !downHit)
            {
                OnAirHit();
                MantainOnAir();
            }
            else if (IsGrounded)
            {
                OnHitStunt();
            }

            if (downHit)
            {
                GetToTheGround();
            }
        }
        if (Life <= 0)
        {
            if (Cell != null)
            {
                Cell.OnEnemyKilledInside(gameObject);
            }
            _gravityValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
            if (_lifeOrbPrefab != null)
            {
               StartCoroutine(SpawnOrbs());
            }
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection, 15);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, gameObject, EnemyCatalogue.Esqueleton);
            Debug.Log("Murio");
            _isDead = true;
        }
        else
        {
            if (pushDirection != Vector3.zero && IsGrounded)
            {
                _rb.AddForce(pushDirection * 500, ForceMode.Impulse);
            }
        }

    }
    IEnumerator Restart()
    {
        yield return new WaitForSeconds(15);

        GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        _isDead = false;
        Stuned = false;
        UseGravity = true;
        _gravityValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
        Dir = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);

        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;

        GenericFactory.Instance.ReturnObj(EnemyCatalogue.SavageDog, this);
    }
    IEnumerator SpawnOrbs()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 5;
            LifeOrb p = Instantiate(_lifeOrbPrefab, transform.position + Vector3.up * 1.2f, transform.rotation);
            p.transform.parent = GameManager.Instance.Gameplay;
            p.Amount = Random.Range(50, 75);
            GameManager.Instance.LaunchProjectile(p.gameObject, transform.position + new Vector3(offset.x, 0, offset.y));
            yield return new WaitForSeconds(0.5f);
        }
        StartCoroutine(Restart());
    }
    public void Attack()
    {
        if (_attackTimer >= _attackDelay)
        {
            _attackTimer = 0;
            OnAttack();
        }
    }
    public void TakeHealt(float amount)
    {
       
    }
    public override void FlyFunct(float height = 4)
    {
        if (_floatRoutine != null)
        {
            StopCoroutine(_floatRoutine);
            _floatRoutine = null;
        }

        _fsm.ChangeState(FsmSavageDog.DogState.OnMidAir);
        UseGravity = false;
        _gravityValue = 0;

        if (!_rb.isKinematic)
            _rb.linearVelocity = Vector3.zero;

        float targetY = transform.position.y + height;

        if (Physics.SphereCast(transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius, Vector3.up, out RaycastHit hit, height + 0.5f, layerMask: GroundLayer))
            targetY = hit.point.y - _ceilingOffset;

        GetToAir();

        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        _floatRoutine = StartCoroutine(GoUpAndFloat(targetY));
    }

    private IEnumerator GoUpAndFloat(float targetY)
    {
        float tolerance = 0.05f;
        float speed = 50f;

        while (Mathf.Abs(transform.position.y - targetY) > tolerance)
        {
            if (GameManager.Instance.IsPaused)
            {
                yield return null;
                continue;
            }

            if (!_rb.isKinematic)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
                _rb.MovePosition(pos);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.linearVelocity = Vector3.zero;
        _gravityValue = 0;
        UseGravity = true;
        _floatRoutine = null;
        OnFreeFall();
    }
    private void MantainOnAir()
    {
        print("MantainOnAir");
        _gravityValue = 0;
        _rb.AddForce(Vector3.up * 200, ForceMode.Impulse);
    }
    public override void GetToTheGround()
    {
        if (_floatRoutine != null)
        {
            StopCoroutine(_floatRoutine);
            _floatRoutine = null;
        }

        GetToGround();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.linearVelocity = Vector3.zero;
        _gravityValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
        UseGravity = true;

        _rb.AddForce(-Vector3.up * _groundImpulse, ForceMode.Impulse);
    }
 
    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.green;
        PathNode ant = null;
        if (_paths.Count > 0) { Gizmos.DrawRay(transform.position, _paths[0].transform.position - transform.position); }
        foreach (PathNode i in _paths)
        {
            if (ant != null)
            {
                Gizmos.DrawRay(ant.transform.position, i.transform.position - ant.transform.position);
            }
            ant = i;
        }*/
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
       /* Gizmos.color=Color.red;
        Gizmos.DrawRay(transform.position, Dir*Dir.magnitude);*/
    }
}
