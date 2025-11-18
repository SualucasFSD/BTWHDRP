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
    public FsmSavageDog _fsm = new FsmSavageDog();
    private bool _isReady = false;
    //[SerializeField] private Collider _collider;
    [Header("Debug")]
    public List<PathNode> _paths = new List<PathNode>();
    [Header("Variables")]
    [SerializeField] private ParticleSystem _bloodVfx;
    [SerializeField] private LifeOrb _lifeOrbPrefab;
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private float _ceilingOffset;
    [SerializeField] private float _groundImpulse = 2500f;
    [SerializeField] private float _attackDelay = 2.5f;
    private float _attackTimer;
    public bool CanAttack = true;
    //public bool IsStanding = true;
    private bool _isDead = false;
    #region Events
    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAttack = delegate { };
    public event Action OnAirHit = delegate { };
    public event Action OnHitStunt = delegate { };
    public event Action GetToAir = delegate { };
    public event Action GetToGround = delegate { };
    public event Action OnFreeFall = delegate { };
    public event Action<bool> OnGrounded = delegate { };
    #endregion

    private void Awake()
    {
        Kind = KindOfEntity.Enemy;
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
    }
    private void OnEnable()
    {
        if (_isReady)
        {
            _attackTimer = _attackDelay;
            _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
            GameManager.Instance.AddEntity(this, Kind);
        }
    }
    /*public override void EnableAgain()
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
        }
        GameManager.Instance.AddEntity(this, Kind);
    }*/
    private void Start()
    {
        GravValue= GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
        GameManager.Instance.AddEntity(this, Kind);
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;
        _fsm.AddState(FsmSavageDog.DogState.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat), OnMovePj, OnRotatePj, EnemyCatalogue.SavageDog));
        _fsm.AddState(FsmSavageDog.DogState.OnCombat, new SavageDogOnCombat(_fsm, this));
        _fsm.AddState(FsmSavageDog.DogState.OnMidAir, new OnAir(this, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat)));
        _fsm.AddState(FsmSavageDog.DogState.OnGoinAir, new OnGoingAirState(this, () => _fsm.ChangeState(FsmSavageDog.DogState.OnMidAir), 4.5f, _ceilingOffset, _rb));
        _fsm.AddState(FsmSavageDog.DogState.OnGoinGround, new ToTheGroundState(this,_rb,_groundImpulse,EnemyCatalogue.SavageDog, () => _fsm.ChangeState(FsmSavageDog.DogState.OnCombat)));
        _fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
        _isReady = true;
    }
    private void Update()
    {
        if (GameManager.Instance.IsPaused||_isDead||!Ready)
        {
            return;
        }
        if (_attackTimer < _attackDelay)
        {
            _attackTimer += Time.deltaTime;
        }
        if(UseGravity)
        {
            if (GravValue < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce)
            {
                GravValue += Time.deltaTime * 7f;
            }
        }
        OnGrounded(IsGrounded);
        _fsm.ArtificialUpdate();
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused||_isDead||!Ready)
        {
            return;
        }
        IsGroundedDetector();
        if (UseGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(GravValue, 2), ForceMode.Acceleration);
        }
        ApplySeparation();
        _fsm.ArtificialFixedUpdate();
    }
    #region Idamageable
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool airHit = false, bool isStunDamage = false,float pushForce=1000)
    {
      if(_isDead)
      {
        return;
      }
        //Ready = true;
        //EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        if (_bloodVfx != null&&isStunDamage)
        {
            _bloodVfx.Play();
        }
        Life -=dmg;
      if(!downHit&&isStunDamage)
      {
        if(IsGrounded)
        {
          OnHitStunt();
          //Ready = true;
        }
        else
        {
           OnAirHit();
           MantainOnAir();
        }
      }

        if (Life <= 0)
        {
            _isDead = true;
            if (Cell != null)
            {
                Cell.OnEnemyKilledInside(gameObject);
            }
            if (_lifeOrbPrefab != null)
            {
                SpawnOrbsFunct();
            }
            FinishElectricPause();
            PopVenemous();
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection, 15);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, gameObject, EnemyCatalogue.Esqueleton);
            _rb.useGravity=true;
        }
        else
        {
            if (pushDirection != Vector3.zero && IsGrounded)
            {
                _rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
        }
    }

    public void TakeHealt(float amount)
    {
    
    }
    #endregion
    #region Movimiento y Rotacion
    public void OnMovePj()
    {
        if (Tg == null || _attackTimer < _attackDelay || !CanAttack||Stuned)
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
        AddForce(IaMov.Instance.Arrive(this, Tg, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce) * GameManager.Instance.ArrivePriotity);
        _rb.MovePosition(_rb.position+new Vector3(Dir.x, 0, Dir.z)*Time.fixedDeltaTime);
    }
    public void ApplySeparation()
    {
        if(Stuned)
        {
            return;
        }
        Vector3 separationForce = IaMov.Instance.Separation(
            GameManager.Instance.GetSeparationEntityes(),
            1.5f,
            this,
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity,
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce
        )+ IaMov.Instance.ObstacleAvoid(this,
        GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Velocity,
        GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce,
        GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius);

        if (separationForce.sqrMagnitude > 0.001f)
        {
            AddForce(separationForce*GameManager.Instance.SeparationPriority);
            //OnMove(Dir);
            _rb.MovePosition(_rb.position + new Vector3(Dir.x, 0, Dir.z) * Time.fixedDeltaTime);
        }
    }
    public void OnRotatePj(Vector3 Direction)
    {
        if (_rb == null||Stuned) return;
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
    #endregion
    #region Player Effectors to Movement
    public override void FlyFunct()
    {
        if (Life <= 0)
        {
            return;
        }
        //Ready = true;
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        _fsm.ChangeState(FsmSavageDog.DogState.OnGoinAir);
        GetToAir();
    }
    private void MantainOnAir()
    {
        if (Life <= 0)
        {
            return;
        }
        print("MantainOnAir");
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        GravValue =0;
        _rb.AddForce(Vector3.up * 280, ForceMode.Impulse);
    }
    public override void GetToTheGround()
    {
        if (Life <= 0)
        {
            return;
        }
        //Ready = true;
        _fsm.ChangeState(FsmSavageDog.DogState.OnGoinGround);
        GetToGround();
    }

    #endregion
    #region Variety of Functs
    public void Attack()
    {
        if (_attackTimer >= _attackDelay)
        {
            _attackTimer = 0;
            OnAttack();
        }
    }
    private void SpawnOrbsFunct()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 5;
            LifeOrb p = Instantiate(_lifeOrbPrefab, transform.position + Vector3.up * 1.2f, transform.rotation);
            p.transform.parent = GameManager.Instance.Gameplay;
            p.Amount = Random.Range(50, 75);
            p.GetComponent<Rigidbody>().AddForce((Vector3.up*100)+new Vector3(offset.x,0,offset.y)*100);
        }
        StartCoroutine(Restart());
    }
    IEnumerator Restart()
    {
        yield return new WaitForSeconds(15);

        GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        //_isDead = false;
        //Stuned = false;
        //UseGravity = true;
        //_rb.useGravity = false;
        //GravValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
        //Dir = Vector3.zero;
        //_rb.linearVelocity = Vector3.zero;
        //_rb.angularVelocity = Vector3.zero;
        ////_fsm.ChangeState(FsmSavageDog.DogState.OnPatrol);
        //Ready = true;
        //Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;

        GenericFactory.Instance.ReturnObj(EnemyCatalogue.SavageDog, this);
    }
    #endregion
    private void OnDisable()
    {
        //GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        _isDead = false;
        Stuned = false;
        UseGravity = true;
        _rb.useGravity = false;
        GravValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].GravityForce;
        Dir = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        Ready = true;
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Life;
    }
}