using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(Rigidbody))]
public class SavageDog : Entity, Idamageable
{
    private Rigidbody _rb;
    public bool Stuned = false;
    private bool _inAirCombo=false;
    private Coroutine _orbsRoutine;
    private float _groundDelay=0;
    private Coroutine _floatRoutine;
    public FsmSavageDog _fsm=new FsmSavageDog();
    private bool _isReady=false;
    [SerializeField]private Collider _collider;
    [Header("Debug")]
    public List<PathNode> _paths = new List<PathNode>();
    [Header("Variables")]
    [SerializeField] private LifeOrb _lifeOrbPrefab;
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private PhysicMaterial _movMat;
    [SerializeField] private PhysicMaterial _stopMat;
    [SerializeField] private float _ceilingOffset;
    [SerializeField] private float _groundImpulse=2500f;
    [SerializeField] private float _attackDelay=2.5f;
    public bool UseGravity=true;
    public bool CanMove = true;
    private float _gravityValue;
    private float _attackTimer;
    //[SerializeField] private float _velocity;
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
        Life= GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;
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
        _attackTimer = _attackDelay;
        if (_isReady)
        {
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
        }
        GameManager.Instance.AddEntity(this, Kind);
    }
    private void Start()
    {
        _fsm.AddState(FsmSavageDog.DogState.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat), OnMovePj,OnRotatePj, _rb,EnemyCatalogue.SavageDog));
        _fsm.AddState(FsmSavageDog.DogState.OnCombat, new SavageDogOnCombat(_fsm,this));
        _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
    }
    private void Update()
    {
        if(_attackTimer<_attackDelay)
        {
            _attackTimer += Time.deltaTime;
        }
        if(_gravityValue< GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce)
        {
            _gravityValue += Time.deltaTime*7f;
        }
        _groundDelay += Time.deltaTime;
        /*if (_groundDelay > 1.5f)
        {
            //Stuned=false;
            OnGrounded(IsGrounded);
        }*/
        
        OnGrounded(IsGrounded);
        /*if (Stuned)
        {
            _rb.angularVelocity = Vector3.zero;
            return;
        }*/
        _fsm.ArtificialUpdate();
    }
    private void FixedUpdate()
    {
        IsGroundedDetector();
        if (UseGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(_gravityValue, 2), ForceMode.Acceleration);
        }
        /*if (Stuned)
        {
            return;
        }*/
        _fsm.ArtificialFixedUpdate();
        if (Dir == Vector3.zero)
        {
            _rb.angularVelocity = Vector3.zero;
            return;
        }
    }
    public void OnMovePj(Transform tg)
    {
        if(Stuned)
        { return; }
        if (tg == null||_attackTimer<_attackDelay)
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
        AddForce(IaMov.Instance.Arrive(this, tg, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce));
        if (OnMove != null)
        {
            OnMove(Dir);
        }
        Vector3 velocityChange = (Dir * GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity) - new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _rb.AddForce(velocityChange * 50, ForceMode.Acceleration);
    }
    public void OnRotatePj(Vector3 Direction)
    {
        if (Direction.sqrMagnitude > 0.001f)
        {
            Vector3 flatDir = new Vector3(Direction.x, 0f, Direction.z).normalized;

            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
                Quaternion deltaRot = targetRot * Quaternion.Inverse(_rb.rotation);

                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;

                if (Mathf.Abs(angle) > 1f)
                {
                    float rotForce = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce;

                    Vector3 torqueP = axis.normalized * angle * rotForce;

                    Vector3 torqueD = -_rb.angularVelocity * 10f;

                    _rb.AddTorque(torqueP + torqueD, ForceMode.Acceleration);
                }
                else
                {
                    _rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
    private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0) { return; }
        Dir = Vector3.ClampMagnitude(Dir + target, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity);
    }
    private void OnDisable()
    {
        //_fsm.ChangeState(FsmMague.MagueStates.OnDeath);
        GameManager.Instance.RemoveEntity(this, Kind);
    }
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection)
    {
        if (Life <= 0)
        {
            return;
        }
        /*if (!IsDamageable) { return; }
        _stuntPercent += stunt;*/
        Life -= dmg;
        if (!IsGrounded)
        {
            MantainOnAir();
        }
        //_fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);
        //SoundManager.Instance.PlayOneShot(entityType.basic, soundType.attack, _mySource);
        //_damageParticles.Play();
        //lifebar.value=life/maxlife;
        if (Life <= 0 && _orbsRoutine == null)
        {
            _gravityValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
            _collider.material = _stopMat;
            if (_lifeOrbPrefab != null)
            {
                _orbsRoutine = StartCoroutine(SpawnOrbs());
            }
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection, 50);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, gameObject, EnemyCatalogue.Esqueleton);
            enabled = false;
            //gameObject.SetActive(false);
            //_fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnPatrol);
            //StartCoroutine(Restart());
        }
        else
        {
            if (pushDirection != Vector3.zero && IsGrounded)
            {
                _rb.AddForce(pushDirection * 500, ForceMode.Impulse);
            }
        }
       /* if (_stuntPercent >= GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].StuntResistance)
        {
            _stuntPercent = 0;
            _fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnStunt);
        }*/
        if (IsGrounded&&!_inAirCombo)
        {
            OnHitStunt();
        }
        else
        {
            OnAirHit();
            MantainOnAir();
        }

    }
    IEnumerator SpawnOrbs()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 5;
            LifeOrb p = Instantiate(_lifeOrbPrefab, transform.position + Vector3.up * 1.2f, transform.rotation);
            p.transform.parent = GameManager.Instance.Gameplay;
            p.Amount = Random.Range(15, 25);
            GameManager.Instance.LaunchProjectile(p.gameObject, transform.position + new Vector3(offset.x, 0, offset.y));
            yield return new WaitForSeconds(0.5f);
        }
        _orbsRoutine = null;
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
        Stuned = true;
        //_fsm.ChangeState(FsmEnemyEsqueleton.AgentStates.OnTakeDamage);

        UseGravity = false;
        if (!_rb.isKinematic)
        {
            _rb.velocity = Vector3.zero;
        }

        float targetY = transform.position.y + height;

        if (Physics.SphereCast(transform.position, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius, Vector3.up, out RaycastHit hit, height+0.5f, layerMask: GroundLayer))
        {
            targetY = hit.point.y - _ceilingOffset;
        }

        //gameObject.layer = _airLayer;
        GetToAir();
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        _floatRoutine=StartCoroutine(GoUpAndFloat(targetY));
    }
    private void MantainOnAir()
    {
        _gravityValue = 0;
    }
    private IEnumerator GoUpAndFloat(float targetY)
    {
        _collider.material=_stopMat;
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
        _gravityValue = 0;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        OnFreeFall();
        while (_gravityValue < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            yield return null;
        }
        _groundDelay = 0;
        //OnFreeFall();
        //Stuned = false;
        UseGravity = true;
    }
    public override void GetToTheGround()
    {
      if (_floatRoutine!=null)
      {
         StopCoroutine(_floatRoutine);
         _floatRoutine = null;
      }
        GetToGround();  
        _groundDelay = 0;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.AddForce(-Vector3.up * _groundImpulse, ForceMode.Impulse);
        //Stuned = false;
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
