using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class MagueEnemyModel : Entity, Idamageable
{
    [Header("References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Collider _collider;
    [SerializeField] private ParticleSystem _damageParticles;
    [SerializeField] private AudioSource _mySource;

    [Header("Stats")]
    [SerializeField] private float _ceilingOffset = 1.5f;
    [SerializeField] private float _groundImpulse = 2500f;
    [SerializeField] private LayerMask _nodeLayer;

    [Header("Prefabs")]
    [SerializeField] private MagueBullet _bulletPrefab;
    [SerializeField] private Transform[] _bulletPos = new Transform[3];
    [SerializeField] private LifeOrb _lifeOrbPrefab;

    public bool IsCharging = false;

    public int NumbOfBullets = 0;
    //public bool IsStanding = true;
    private bool _isReady = false;
    private bool _isDead = false;
    private List<MagueBullet> Bullets = new List<MagueBullet>();

    public FsmMague _fsm = new FsmMague();

    public event Action<Vector3> OnMove = delegate { };
    public event Action OnAttack = delegate { };
    public event Action OnCharging = delegate { };
    public event Action<float> OnDamage = delegate { };
    public event Action OnDeath = delegate { };
    public event Action GetToAir = delegate { };
    public event Action OnFreeFall = delegate { };
    public event Action GetToGround = delegate { };
    public event Action OnAirHit = delegate { };
    public event Action OnHitStunt = delegate { };
    public event Action<bool> OnGround = delegate { };
    private void Awake()
    {
        Kind = KindOfEntity.Enemy;
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
    }

    public override void EnableAgain()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null;
        if (_isReady)
        {
            _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
            Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;
        }
        GameManager.Instance.AddEntity(this, Kind);
    }

    private void Start()
    {
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;
        GameManager.Instance.AddEntity(this, Kind);
        _fsm.AddState(FsmMague.MagueStates.OnPatrol, new OnPatrol(this, _nodeLayer, () => _fsm.ChangeState(FsmMague.MagueStates.OnCombat), OnMovePj, OnRotatePj, EnemyCatalogue.Mague));
        _fsm.AddState(FsmMague.MagueStates.OnCombat, new OnCombatMague(_fsm, this));
        _fsm.AddState(FsmMague.MagueStates.OnMidAir, new OnAir(this, () => _fsm.ChangeState(FsmMague.MagueStates.OnCombat)));
        _fsm.AddState(FsmMague.MagueStates.OnGetAir, new OnGoingAirState(this, () => _fsm.ChangeState(FsmMague.MagueStates.OnMidAir), 4.5f, _ceilingOffset, _rb));
        _fsm.AddState(FsmMague.MagueStates.OnGetGround, new ToTheGroundState(this, _rb, _groundImpulse, EnemyCatalogue.Mague, () => _fsm.ChangeState(FsmMague.MagueStates.OnCombat)));
        _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        _isReady = true;
    }
    private void Update()
    {
        if (GameManager.Instance.IsPaused || _isDead || !Ready)
        {
            return;
        }
       
        if(UseGravity)
        {
            if (GravValue < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].GravityForce)
            {
                GravValue += Time.deltaTime * 7f;
            }
        }
        OnGround(IsGrounded);
        _fsm.ArtificialUpdate();
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused || _isDead || !Ready)
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
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool airHit = false, bool isStunDamage = false, float pushForce = 1000)
    {
        if(_isDead)
        {
            return;
        }
        //Ready = true;
        //EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        if (_damageParticles != null)
        {
            _damageParticles.Play();
        }
        BulletsStop();
        Life -= dmg;
        if (!downHit && isStunDamage)
        {
            if (IsGrounded)
            {
                OnHitStunt();
            }
            else
            {
                OnAirHit();
                MantainOnAir();
            }
        }
        if (Life<=0)
        {
            _isDead = true;
            BulletsStop();
            if (Cell != null)
            {
                Cell.OnEnemyKilledInside(gameObject);
                Cell = null;
            }
           if (_lifeOrbPrefab != null)
            {
                SpawnOrbsFunct();
            }
           FinishElectricPause();
           PopVenemous();
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection, 15);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, gameObject, EnemyCatalogue.Mague);
            _rb.useGravity = true;
        }
        else
        {
            if (pushDirection != Vector3.zero && IsGrounded)
                _rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
    }

    public void TakeHealt(float amount)
    {
       
    }
    #endregion
    #region Movimiento y Rotacion
    public void OnMovePj()
    {
        if (Tg == null || Stuned||IsCharging)
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
        AddForce(IaMov.Instance.Arrive(this, Tg, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce) * GameManager.Instance.ArrivePriotity);
        _rb.MovePosition(_rb.position + new Vector3(Dir.x, 0, Dir.z) * Time.fixedDeltaTime);
    }
    public void ApplySeparation()
    {
        if (Stuned)
        {
            return;
        }
        Vector3 separationForce = IaMov.Instance.Separation(
            GameManager.Instance.GetSeparationEntityes(),
            1.5f,
            this,
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity,
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce
        ) + IaMov.Instance.ObstacleAvoid(this,
        GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity,
        GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce,
        GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius);

        if (separationForce.sqrMagnitude > 0.001f)
        {
            AddForce(separationForce * GameManager.Instance.SeparationPriority);
            //OnMove(Dir);
            _rb.MovePosition(_rb.position + new Vector3(Dir.x, 0, Dir.z) * Time.fixedDeltaTime);
        }
    }
    public void OnRotatePj(Vector3 Direction)
    {
        if (_rb == null || Stuned) return;
        if (Direction.sqrMagnitude <= 0.001f) return;

        Direction.y = 0f;
        if (Direction.sqrMagnitude < 0.0001f) return;

        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(Direction.normalized, Vector3.up), GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce * Time.fixedDeltaTime));
    }
    private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0) { Dir = Vector3.zero; return; }
        Dir = Vector3.ClampMagnitude(Dir + target, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity);
    }
    private void SpawnOrbsFunct()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 5;
            LifeOrb p = Instantiate(_lifeOrbPrefab, transform.position + Vector3.up * 1.2f, transform.rotation);
            p.transform.parent = GameManager.Instance.Gameplay;
            p.Amount = Random.Range(50, 75);
            p.GetComponent<Rigidbody>().AddForce((Vector3.up * 100) + new Vector3(offset.x, 0, offset.y) * 100);
        }
        StartCoroutine(Restart());
    }
    IEnumerator Restart()
    {
        yield return new WaitForSeconds(15);

        GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        _isDead = false;
        Stuned = false;
        UseGravity = true;
        _rb.useGravity = false;
        GravValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].GravityForce;
        Dir = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);

        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;

        GenericFactory.Instance.ReturnObj(EnemyCatalogue.Mague, this);
    }
    #endregion
    #region Player Effectors to Movement
    public override void FlyFunct()
    {
        if (_isDead)
        {
            return;
        }
        BulletsStop();
        Ready = true;
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        _fsm.ChangeState(FsmMague.MagueStates.OnGetAir);
        GetToAir();
    }
    private void MantainOnAir()
    {
        if (_isDead)
        {
            return;
        }
        print("MantainOnAir");
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        GravValue = 0;
        _rb.AddForce(Vector3.up * 280, ForceMode.Impulse);
    }
    public override void GetToTheGround()
    {
        if (_isDead)
        {
            return;
        }
        BulletsStop();
        Ready = true;
        _fsm.ChangeState(FsmMague.MagueStates.OnGetGround);
        GetToGround();
    }
    #endregion
    #region Mague Functs
    public void StartCharging()
    {
      if (Stuned || !IsGrounded) return;
        IsCharging = true;
       OnCharging?.Invoke();
    }
    public void BulletsStop()
    {
        foreach (MagueBullet b in Bullets)
        {
            GameObjectFactory.Instance.ReturnObj(GenericObjectType.MagueBullet, b.gameObject);
        }
            //Destroy(b.gameObject);
        NumbOfBullets = 0;
        Bullets.Clear();
    }
    public void MagicInstance(Transform _tg)
    {
        //Bullets.Add(Instantiate(_bulletPrefab, _bulletPos[NumbOfBullets].position, transform.rotation));
        GameObject P = GameObjectFactory.Instance.GetObj(GenericObjectType.MagueBullet, _bulletPos[NumbOfBullets].position, transform.rotation);
        Bullets.Add(P.GetComponent<MagueBullet>());
        Bullets[NumbOfBullets].SetTarget(_tg);
        Bullets[NumbOfBullets].Kind = Kind;
        NumbOfBullets++;
    }

    public void Shoot()
    {
      IsCharging = false;
      foreach (MagueBullet b in Bullets)
      b.Fire = true;
      Bullets.Clear();
      NumbOfBullets = 0;
      OnAttack?.Invoke();
    }
    #endregion
}
