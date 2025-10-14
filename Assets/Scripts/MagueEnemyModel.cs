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
    /*[SerializeField] private PhysicsMaterial _stopMat;
    [SerializeField] private PhysicsMaterial _movMat;*/
    [SerializeField] private VisualEffect _damageParticles;
    [SerializeField] private AudioSource _mySource;

    [Header("Stats")]
    [SerializeField] private float _ceilingOffset = 1.5f;
    [SerializeField] private float _groundImpulse = 2500f;
    [SerializeField] private LayerMask _nodeLayer;

    [Header("Prefabs")]
    [SerializeField] private MagueBullet _bulletPrefab;
    [SerializeField] private Transform[] _bulletPos = new Transform[3];
    [SerializeField] private LifeOrb _lifeOrbPrefab;

    public bool Stuned = false;
    public bool UseGravity = true;
    public bool IsCharging = false;

    public int NumbOfBullets = 0;
    private bool _isReady = false;
    private float _gravityValue;
    private bool _isDead=false;
    private Coroutine _floatRoutine;
    private List<MagueBullet> Bullets = new List<MagueBullet>();

    public FsmMague _fsm = new FsmMague();

    // Eventos 
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
            _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    /*private void OnEnable()
    {
        if (gameObject.activeInHierarchy)
        {
            if (_isReady)
            {
                _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
                Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;
            }
            GameManager.Instance.AddEntity(this, Kind);
        }
    }*/
    private void OnEnable()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayedInit());
        }
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
        _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);
        _isReady = true;
    }

    private void Update()
    {
        if(_isDead)
        {
            return;
        }
        _fsm.ArtificialUpdate();

        if (_gravityValue < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].GravityForce)
            _gravityValue += Time.deltaTime * 7f;

        OnGround(IsGrounded);

        if (Tg != null)
        {
            AddForce(IaMov.Instance.Separation(GameManager.Instance.GetSeparationEntityes(), 1.8f, this,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce)
            + IaMov.Instance.Arrive(this, Tg,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce) * 0.8f
            + IaMov.Instance.ObstacleAvoid(this,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].RotForce,
                GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius));
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
            _rb.AddForce(-transform.up * Mathf.Pow(_gravityValue, 2), ForceMode.Acceleration);

        _fsm.ArtificialFixedUpdate();

        if (Dir == Vector3.zero && !IsCharging)
        {
            _rb.angularVelocity = Vector3.zero;
            return;
        }
    }

    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection)
    {
        if (Life <= 0)
            return;

        Life -= dmg;
        BulletsStop();
        if (!IsGrounded)
        {
            OnAirHit();
            MantainOnAir();
        }
        else
        {
            Stuned = true;
            OnHitStunt();
        }

        _damageParticles.Play();

        if (Life <= 0)
        {
            if(Cell!=null)
            {
                Cell.OnEnemyKilledInside(gameObject);
            }
            _gravityValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].GravityForce;
            if (_lifeOrbPrefab != null)
            {
                StartCoroutine(SpawnOrbs());
            }
            BulletsStop();
            GameManager.Instance.RemoveEntity(this, Kind);
            GetComponentInChildren<RagdollOnOff>().RagdollModeOn(pushDirection, 15);
            EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, gameObject, EnemyCatalogue.Mague);
            Cell = null;
            _isDead=true;
        }
        else
        {
            if (pushDirection != Vector3.zero && IsGrounded)
                _rb.AddForce(pushDirection * 500, ForceMode.Impulse);
        }
    }

    public void BulletsStop()
    {
        foreach (MagueBullet b in Bullets)
        {
            Destroy(b.gameObject);
        }
        NumbOfBullets = 0;
        Bullets.Clear();
    }

    IEnumerator Restart()
    {
        /*yield return new WaitForSeconds(15);
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;
        GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        _isDead = false;
        GenericFactory.Instance.ReturnObj(EnemyCatalogue.Mague, this);*/
        yield return new WaitForSeconds(15);

        GetComponentInChildren<RagdollOnOff>().RagdollModeOff();
        _isDead = false;
        Stuned = false;
        UseGravity = true;
        _gravityValue = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].GravityForce;
        Dir = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _fsm.ChangeState(FsmMague.MagueStates.OnPatrol);

        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Life;

        GenericFactory.Instance.ReturnObj(EnemyCatalogue.Mague, this);
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

    public void MagicInstance(Transform _tg)
    {
        Bullets.Add(Instantiate(_bulletPrefab, _bulletPos[NumbOfBullets].position, transform.rotation));
        Bullets[NumbOfBullets].Tg = _tg;
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

    public void StartCharging()
    {
        if (Stuned || !IsGrounded) return;
        IsCharging = true;
        OnCharging?.Invoke();
    }

    public void OnMovePj()
    {
        if (Tg == null || IsCharging)
        {
            Dir = Vector3.zero;
            OnMove(Vector3.zero);
            //_collider.material = _stopMat;
            return;
        }

        //_collider.material = _movMat;
        OnMove(Dir);

        Vector3 velocityChange = (new Vector3(Dir.x, 0, Dir.z) *
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity)
            - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        _rb.AddForce(velocityChange, ForceMode.Acceleration);
    }

    public void OnRotatePj(Vector3 Direction)
    { 
        if (_rb == null) return;
        if (Direction.sqrMagnitude <= 0.001f) return;

        Direction.y = 0f;
        if (Direction.sqrMagnitude < 0.0001f) return;

        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(Direction.normalized, Vector3.up), GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].RotForce * Time.fixedDeltaTime));
    }

    public override void FlyFunct(float height = 4)
    {
        BulletsStop();
        Stuned = true;
        _fsm.ChangeState(FsmMague.MagueStates.OnMidAir);
        UseGravity = false;
        //_collider.material = _stopMat;

        if (!_rb.isKinematic)
            _rb.linearVelocity = Vector3.zero;

        float targetY = transform.position.y + height;

        if (Physics.SphereCast(transform.position,
            GameManager.Instance.EnemyConfiguration[EnemyCatalogue.SavageDog].Radius,
            Vector3.up, out RaycastHit hit, height + 0.5f, GroundLayer))
        {
            targetY = hit.point.y - _ceilingOffset;
        }

        GetToAir();
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        _floatRoutine = StartCoroutine(GoUpAndFloat(targetY));
    }

    private void MantainOnAir()
    {
        _gravityValue = 0;
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
        }
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
        UseGravity = true;
        _gravityValue = 0;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        OnFreeFall();
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
        _rb.AddForce(-Vector3.up * _groundImpulse, ForceMode.Impulse);
        UseGravity = true;
    }

    public void TakeHealt(float amount) { }

    private void AddForce(Vector3 target)
    {
        if (target.magnitude == 0) return;
        Dir = Vector3.ClampMagnitude(Dir + target, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Velocity);
    }
}

