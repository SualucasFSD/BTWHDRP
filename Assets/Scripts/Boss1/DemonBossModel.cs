using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class DemonBossModel : Entity, Idamageable
{
    [SerializeField] FadeinOut _fade;
    [SerializeField] AcquireAbility _chooseAbility;

    [Header("General Components and Values")]
    [SerializeField][Range(0, 1000)] private float _shieldLife = 1000;
    //[SerializeField] private GameObject _victoryPanel;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private ParticleSystem _bloodVfx;
    [SerializeField] private float _rotationForce;
    [SerializeField] private float _moveForce;
    [SerializeField] private float _groundImpulse = 2500f;
    [SerializeField] private Image _shieldPercent;
    [SerializeField] private GameObject _shieldPercentToClose;
    [SerializeField] private ParticleSystem _shieldDmg;
    [SerializeField] private ParticleSystem _shieldBrokeEffect;
    [SerializeField] private Transform _centerPoint;
    [SerializeField] private GameObject _rocksHide;
    [SerializeField] private GameObject _rocksDestroyParticles;
    [SerializeField] private float _rockSpawnRadius = 6f;
    [SerializeField] private float _rockRiseHeight = 4f;
    [SerializeField] private float _rockRiseDuration = 1.5f;
    [SerializeField] private float _explosionChargeTime = 3f;
    [SerializeField] private GameObject _bigThunderWave;
    [SerializeField] private Image _healtBar;
    [SerializeField] private GameObject lifebarToClose;

    [Header("Ray Throw")]
    [SerializeField] private GameObject _rayPrefab;
    [SerializeField] private Transform[] _rayThrowPoints = new Transform[4];
    [SerializeField] private Transform[] _rayDashPoints = new Transform[16];
    private float _maxLife;
    private bool _waitingToShootRay = false;
    private bool _performingRaySequence = false;

    [Header("Dash")]
    [SerializeField] private float _dashForce;
    //[SerializeField] private ParticleSystem _impulseParticle;
    //[SerializeField] private ParticleSystem _dashTrail;
    private Transform _currentDashTarget;
    private int _dashCounter = 0;
    private bool _isRotatingToDash = false;
    private bool _isDashing = false;
    private float _rotationAngleThreshold = 4f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _stunDuration = 3f;
    [SerializeField] private Vector3 _boxHalfExtents = new Vector3(2f, 2f, 2f);

    [Header("ExplosionCharge")]
    [SerializeField] GameObject _chargeParticle;
    [SerializeField] private float _chargeDuration;
    //[SerializeField] private ParticleSystem _chargeParticle;
    [SerializeField] private ParticleSystem _explosionParticle;
    [SerializeField] private float _areaDamage;
    [SerializeField] private LayerMask _explosionLayerMask;
    private bool explosionUsed45 = false;
    private bool explosionUsed20 = false;

    //PRIVATES GENERIC
    [SerializeField] private float _multiRayRotateForce = 8f;
    private bool _isPerformingMultiRayCombo = false;
    private bool _rotationActivate = false;
    private bool _moveActivate = false;
    private bool _isShieldCharge = true;
    private float _predictionTime = 0.5f;
    private Vector3 _lastPos;
    private Vector3 _tgPos;
    private FsmDemonBoss _fsm = new FsmDemonBoss();
    private GameObject _tgNoPredict;
    private bool _isPerformingExplosion = false;
    private List<GameObject> _activeRocks = new List<GameObject>();
    private List<RayShoot> _spawnedRays = new List<RayShoot>();
    private HashSet<RayShoot> _activatedRays = new HashSet<RayShoot>();
    private int _closeAttackCounter = 0;
    private bool _isDoingCloseCombo = false;
    private bool _shootRay = false;
    private bool _fistToFistCombo = false;
    private bool _isdead = false;
    private Coroutine _shieldCharge;
    private bool _isDashingRoutine=false;
    private float _floorHeight=0;
    private float _itiFloor = 0;

    // COROUTINES REFERENCES TO SAFE STOP
    private Coroutine _closeComboCoroutine;
    private Coroutine _explosionCoroutine;
    private Coroutine _multiRayCoroutine;
    private Coroutine _rayBeforeNextDashCoroutine;

    //EVENTOS
    public event Action PrepareImpulse = delegate { };
    public event Action Impulse = delegate { };
    public event Action DashFin = delegate { };
    public event Action<int> Idle = delegate { };
    public event Action<int> ChargeRay = delegate { };
    public event Action<int> ShootRay = delegate { };
    public event Action<bool> Grounded = delegate { };
    public event Action<Vector3> OnMove = delegate { };
    public event Action GetToTheAir = delegate { };
    public event Action GetToGround = delegate { };
    public event Action OnAirHit = delegate { };
    public event Action OnHitStunt = delegate { };
    public event Action OnAttackClose = delegate { };
    public event Action OnMaxHeight = delegate { };
    public event Action Jump = delegate { };
    public event Action JumpPrepare = delegate { };
    public event Action FallExplo = delegate { };
    public event Action PrepareExplosion = delegate { };
    public event Action FinishExplosion = delegate { };
    public event Action<bool> FuriousWalk = delegate { };
    public event Action OnDeath = delegate { };
    public event Action OnStunt = delegate { };

    #region MonoBehaviours
    private void Awake()
    {
        IsRayStunable = false;
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false;
    }

    private void Start()
    {
        GameManager.Instance.AddEntity(this, KindOfEntity.Enemy);
        if (lifebarToClose != null)
        {
            lifebarToClose.SetActive(true);
        }
        if (_shieldPercentToClose != null)
        {
            _shieldPercentToClose.SetActive(true);
        }
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].Life;
        _maxLife = Life;
        EventManager.Suscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);

        _fsm.AddState(FsmDemonBoss.AgentStates.OnGoinAir, new OnGoingAirState(this, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnMidAir), 4.5f, GroundDistanceDetector, _rb));
        _fsm.AddState(FsmDemonBoss.AgentStates.OnMidAir, new OnAir(this, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat)));
        _fsm.AddState(FsmDemonBoss.AgentStates.OnGoinGround, new ToTheGroundState(this, _rb, _groundImpulse, EnemyCatalogue.DemonBoss, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat)));
        _fsm.AddState(FsmDemonBoss.AgentStates.OnCombat, new DashStateDemonBoos());

        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat);
    }

    private void Update()
    {
        if (_isdead)
        {
            return;
        }

        _tgNoPredict = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(Kind), transform);

        if (GameManager.Instance.IsPaused)
        {
            return;
        }
        if (UseGravity)
        {
            float maxGrav = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].GravityForce;
            if (GravValue < maxGrav)
            {
                GravValue += Time.deltaTime * 7f;
            }
        }

        if(_itiFloor<3)
        {
            _itiFloor+=Time.deltaTime;
        }

        if (_isPerformingExplosion ||
            _isPerformingMultiRayCombo ||
            _isDashing ||
            _fistToFistCombo ||
            Stuned ||
            !_isShieldCharge||_isDashingRoutine||_itiFloor<3)
        {
            _fsm.ArtificialUpdate();
            return;
        }

        float hpPercent = Life / GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].Life;

        if (!explosionUsed45 && hpPercent <= 0.45f)
        {
            explosionUsed45 = true;
            StartExplosionCombo();
            _fsm.ArtificialUpdate();
            return;
        }

        if (!explosionUsed20 && hpPercent <= 0.20f)
        {
            explosionUsed20 = true;
            StartExplosionCombo();
            _fsm.ArtificialUpdate();
            return;
        }

        if (hpPercent > 0.70f)
        {
            StartCloseAttackCombo();
            _fsm.ArtificialUpdate();
            return;
        }

        if (hpPercent <= 0.70f)
        {
            float rng = Random.value;

            if (rng <= 0.50f)
            {
                print("CloseCombo");
                StartCloseAttackCombo();
            }
            else
            {
                if (Random.Range(0, 100) > 50)
                {
                    print("4 shoots");
                    StartMultiRayCombo();
                }
                else
                {
                    print("DashShoot");
                    DashShoot();
                }
            }

            _fsm.ArtificialUpdate();
            return;
        }

        _fsm.ArtificialUpdate();
    }

    private void FixedUpdate()
    {
        if (_isdead)
        {
            return;
        }
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        _fsm.ArtificialFixedUpdate();

        IsGroundedDetector();
        if(IsGrounded&&_floorHeight==0&&_itiFloor>=3)
        {
            _floorHeight = transform.position.y;
            print(_floorHeight);
        }
        if (UseGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(GravValue, 2), ForceMode.Acceleration);
        }
        if (Stuned)
        {
            return;
        }
        if (_moveActivate)
        { OnMovePj(); }

        FixedUpdateDash();
    }
    #endregion

    #region Move and Rotate Region
    public void OnMovePj()
    {
        if (_tgPos == Vector3.zero || Stuned)
        {
            Dir = Vector3.zero;
            OnMove(Vector3.zero);
            return;
        }

        if (Vector3.Distance(transform.position, _tgPos) < 1.5f)
        {
            return;
        }
        OnMove(Dir);
        Dir = (_tgPos - transform.position).normalized;
        _rb.MovePosition(_rb.position + new Vector3(Dir.x, 0, Dir.z) * _moveForce * Time.fixedDeltaTime);
    }

    private void RotateToTarget(Vector3 Direction)
    {
        if (_rb == null)
        {
            return;
        }
        Direction -= transform.position;
        Direction.y = 0f;
        if (Direction.sqrMagnitude < 0.0001f)
        { return; }

        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(Direction.normalized, Vector3.up), _rotationForce * Time.fixedDeltaTime));
    }

    private void AreaDamage()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.MakeCameraShake);
        Collider[] c = Physics.OverlapSphere(transform.position, 500, _explosionLayerMask);
        foreach(Collider colision in c)
        {
            if(colision.gameObject==gameObject)
            {
                continue;
            }
            if (!GameManager.Instance.SphereLineOfSight(colision.transform.position, transform.position, 0.5f))
            { continue; }
            if(colision.gameObject.TryGetComponent<Idamageable>(out var Compo))
            {
                Vector3 pushDir = new Vector3(colision.transform.position.x - transform.position.x, 0f, colision.transform.position.z - transform.position.z).normalized;
                Compo.TakeDamage(_areaDamage, 0, pushDir);
            }
        }
    }

    private void TakePjPosition(params object[] p)
    {
        Vector3 playerPos = (Vector3)p[0];

        Vector3 playerVel = (playerPos - _lastPos) / Time.deltaTime;
        _lastPos = playerPos;

        Vector3 predictedPos = playerPos;
        if (playerVel.magnitude > 0.1f)
        { predictedPos += playerVel.normalized * playerVel.magnitude * _predictionTime; }

        Vector3 shootOrigin = transform.position;
        Vector3 dir = (predictedPos - shootOrigin).normalized;
        float dist = Vector3.Distance(shootOrigin, predictedPos);

        if (Physics.Raycast(shootOrigin, dir, out RaycastHit hit, dist, _obstacleMask))
        {
            predictedPos = hit.point;
        }
        _tgPos = predictedPos;
    }
    #endregion

    #region FistToFistCombo
    public void StartCloseAttackCombo()
    {
        if (_isDoingCloseCombo || Stuned)
        {
            return;
        }

        if (_tgNoPredict == null)
        {
            return;
        }

        Idle(2);
        FuriousWalk(true);
        ResetBossState();
        _fistToFistCombo = true;
        _closeAttackCounter = 0;
        if (_closeComboCoroutine != null) StopCoroutine(_closeComboCoroutine);
        _closeComboCoroutine = StartCoroutine(CloseAttackCombo());
    }

    private IEnumerator CloseAttackCombo()
    {
        yield return new WaitForSeconds(1.2f);
        _isDoingCloseCombo = true;
        _moveActivate = true;
        _rotationActivate = true;

        float walkTimer = 0f;
        float maxWalkTime = 5f;
        float dashDistance = 15f;
        float hitDistance = 3f;
        float dashOffset = 2f;

        _closeAttackCounter++;

        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            // exit if shield lost, stunned, or dead
            if (!_isShieldCharge || Stuned || _isdead)
            {
                ResetCloseCombo();
                yield break;
            }

            if (_tgPos == Vector3.zero)
            {
                // si no hay posición prevista, espera un frame
                yield return null;
                continue;
            }

            Vector3 toPlayer = _tgPos - transform.position;
            float distance = toPlayer.magnitude;

            if (_rotationActivate)
            {
                OnMove(Vector3.forward);
                RotateToTarget(_tgPos);
            }

            if (distance <= hitDistance)
            {
                Vector3 forward = transform.forward;
                Vector3 toPlayerDir = toPlayer.normalized;
                float angleToPlayer = Vector3.Angle(forward, toPlayerDir);

                if (angleToPlayer > 20)
                {
                    walkTimer += Time.deltaTime;
                    Dir = toPlayer.normalized;
                    OnMovePj();
                    continue;
                }

                StopMove();
                StopRotate();
                OnMove(Vector3.zero);

                OnAttackClose();

                float wait = 0f;
                while (wait < 4f)
                {
                    yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

                    if (!_isShieldCharge || Stuned)
                    {
                        ResetCloseCombo();
                        yield break;
                    }

                    wait += Time.deltaTime;
                }

                bool repeat = Random.value <= 0.75f;

                if (!repeat || _closeAttackCounter >= 3)
                {
                    ResetCloseCombo(true);
                    yield break;
                }

                walkTimer = 0f;
                _moveActivate = true;
                _rotationActivate = true;
                _closeAttackCounter++;
                continue;
            }

            walkTimer += Time.deltaTime;

            if (walkTimer >= maxWalkTime || distance >= dashDistance)
            {
                Impulse();
                // dash hacia jugador (si existe)
                if (_tgNoPredict != null)
                {
                    yield return DashToPlayer(dashOffset);
                }

                if (!_isShieldCharge || Stuned)
                {
                    ResetCloseCombo();
                    yield break;
                }

                walkTimer = 0f;
                continue;
            }

            Dir = toPlayer.normalized;
            OnMovePj();
        }
    }

    private IEnumerator DashToPlayer(float offset)
    {
        float dashSpeed = 25f;

        if (_tgNoPredict == null)
            yield break;

        Vector3 dir = (_tgNoPredict.transform.position - transform.position).normalized;
        Vector3 target = _tgNoPredict.transform.position - dir * offset;
        target.y = transform.position.y;

        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            if (!_isShieldCharge || Stuned)
            {
                yield break;
            }
            Vector3 current = transform.position;
            Vector3 toTarget = target - current;

            if (toTarget.sqrMagnitude <= 0.2f)
            {
                _rb.MovePosition(target);
                _rb.angularVelocity = Vector3.zero;

                DashFin();
                yield break;
            }

            if (_tgNoPredict == null)
            {
                yield break;
            }

            RotateToTarget(_tgNoPredict.transform.position);

            Vector3 next = Vector3.MoveTowards(current, target, dashSpeed * Time.deltaTime);

            _rb.MovePosition(next);
        }
    }

    private void ResetCloseCombo(bool normalEnd = false)
    {
        // stop and cleanup
        if (_closeComboCoroutine != null)
        {
            StopCoroutine(_closeComboCoroutine);
            _closeComboCoroutine = null;
        }

        StopMove();
        StopRotate();
        _fistToFistCombo = false;
        _isDoingCloseCombo = false;
        _moveActivate = false;
        _rotationActivate = false;

        // reset counter either way (keeps your original behavior)
        _closeAttackCounter = 0;
    }

    private void StopMove()
    {
        _moveActivate = false;
        _rb.angularVelocity = Vector3.zero;
    }

    private void StopRotate()
    {
        _rotationActivate = false;
    }
    #endregion

    #region IdamageableRegion
    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool airHit = false, bool isStuntDamage = false, float pushForce = 1000)
    {
        if (Life <= 0)
        {
            return;
        }
        if (_isShieldCharge)
        {
            _shieldLife -= dmg;

            if (_shieldBrokeEffect != null)
            {
                _shieldDmg.Play();
            }
            if (_shieldPercent != null)
            {
                _shieldPercent.fillAmount = _shieldLife / 1000;
            }
            if (_shieldLife <= 0)
            {

                _shieldBrokeEffect.Play();
                gameObject.layer = 10;
                _isShieldCharge = false;

                StopAllComboCoroutines();
                ResetBossState();

                DestroyActiveRocks();

                foreach (var r in _spawnedRays.ToArray())
                {
                    if (r == null) continue;
                    if (!_activatedRays.Contains(r))
                    {
                        Destroy(r.gameObject);
                    }
                }
                _spawnedRays.Clear();
                _activatedRays.Clear();
                OnMove(Vector3.zero);
                OnStunt();
                Stuned = true;

                _shieldCharge = StartCoroutine(ShieldRechardRoutine());
            }
        }
        else
        {
            if (_bloodVfx != null && isStuntDamage)
            {
                _bloodVfx.Play();
            }

            Life -= dmg;

            if (_healtBar != null)
            {
                _healtBar.fillAmount = Life / _maxLife;
            }

            if (!downHit && isStuntDamage)
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

            if (Life <= 0)
            {
                GameManager.Instance.RemoveEntity(this, KindOfEntity.Enemy);
                _isdead = true;
                StartCoroutine(Ending());
                if (_shieldCharge != null)
                {
                    StopCoroutine(_shieldCharge);
                    _shieldCharge = null;
                }
                _isShieldCharge = false;
               /* if (_victoryPanel != null)
                {
                    _victoryPanel.SetActive(true);
                }*/
               if(_shieldPercentToClose!=null)
               {
                    _shieldPercentToClose.SetActive(false);
               }
                gameObject.layer = 18;
                if (lifebarToClose != null)
                {
                    lifebarToClose.SetActive(false);
                }
                OnStunt();
                OnDeath();
            }
        }
    }

    IEnumerator Ending()
    {
        _fade.StartFadeIn();
        yield return new WaitForSeconds(2);
        _chooseAbility.StartCoroutine(_chooseAbility.OnAbilityAcquired());
        if (SaveSystemManager.instance != null)
        {
            SaveSystemManager.instance._saveDatas[0] = new DataSave();
        }
        yield return new WaitForSeconds(5);
        GameManager.Instance.LoadHub();
    }

    public void TakeHealt(float amount) { }

    IEnumerator ShieldRechardRoutine()
    {
        while (_shieldLife < 1000)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            if (_shieldPercent != null)
            {
                _shieldPercent.fillAmount = _shieldLife / 1000;
            }
            _shieldLife += 10f;
            yield return new WaitForSeconds(0.1f);
        }
        _isShieldCharge = true;
        Stuned = false;
        ResetBossState();
        _shieldCharge = null;
        _currentDashTarget = null;
        _isDoingCloseCombo = false;
        _isPerformingExplosion = false;
        _isPerformingMultiRayCombo = false;
        _waitingToShootRay = false;
        _performingRaySequence = false;
    }
    #endregion

    #region ExplosionCombo
    public void StartExplosionCombo()
    {
        if (_isPerformingExplosion)
        {
            return;
        }
        ResetBossState();
        if (_explosionCoroutine != null) StopCoroutine(_explosionCoroutine);
        _explosionCoroutine = StartCoroutine(ExplosionComboRoutine());
    }

    private IEnumerator ExplosionComboRoutine()
    {
        _isPerformingExplosion = true;
        _moveActivate = false;
        _rotationActivate = false;
        JumpPrepare();

        while (!IsFacingCenter())
        {
            FaceCenter();
            yield return null;
        }

        yield return StartCoroutine(JumpToCenterRoutine());

        if (_bigThunderWave != null)
        {
            Instantiate(_bigThunderWave, transform.position - Vector3.up, Quaternion.identity);
        }
        List<Transform> spawnedRocks = SpawnRocksAroundBoss();

        PrepareExplosion();
        ChargeParticle(true);
        if (_explosionParticle!=null)
        {
            _explosionParticle.Play();
        }
        yield return StartCoroutine(RaiseRocksRoutine(spawnedRocks));

        float timer = 0f;

        //PrepareExplosion();
        while (timer < _explosionChargeTime)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            if (!_isShieldCharge)
            {
                DestroyRocks(spawnedRocks);
                _isPerformingExplosion = false;
                yield break;
            }

            timer += Time.deltaTime;
        }
        ChargeParticle(false);
        FinishExplosion();
        if (_explosionParticle != null)
        {
            _explosionParticle.Play();
        }
        AreaDamage();
        DestroyRocks(spawnedRocks);
        Idle(2);
        yield return new WaitForSeconds(1.5f);
        _isPerformingExplosion = false;
        _explosionCoroutine = null;
    }

    void ChargeParticle(bool state)
    {
         _chargeParticle.SetActive(state);
       
    }

    private bool IsFacingCenter(float toleranceDegrees = 5f)
    {
        Vector3 dir = _centerPoint.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.1f)
            return true;

        Quaternion target = Quaternion.LookRotation(dir.normalized);
        float angle = Quaternion.Angle(transform.rotation, target);

        return angle < toleranceDegrees;
    }
    private void FaceCenter()
    {
        Vector3 dir = _centerPoint.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _rotationForce);
        }
    }

    private IEnumerator JumpToCenterRoutine()
    {
        Vector3 targetPos = _centerPoint.position;
        Vector3 startPos = transform.position;

        float peakHeight = startPos.y + 12f;
        float midHeight = (startPos.y + peakHeight) * 0.5f;

        float ascendSpeed = 45f;
        float fallSpeed = 65f;

        UseGravity = false;
        IsGrounded = false;
        gameObject.layer = 18;
        Jump();
        while (_rb.position.y < peakHeight)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            Vector3 horizontal = targetPos - _rb.position;
            horizontal.y = 0;
            horizontal.Normalize();

            Vector3 finalDir = (horizontal * 0.2f + Vector3.up * 1f).normalized;

            _rb.MovePosition(_rb.position + finalDir * ascendSpeed * Time.deltaTime);
        }
        OnMaxHeight();
        yield return new WaitForSeconds(0.5f);
        FallExplo();
        //Vector3 fallTarget = new Vector3(targetPos.x, startPos.y, targetPos.z);
        Vector3 fallTarget = new Vector3(targetPos.x, _floorHeight, targetPos.z);
        while (_rb.position.y > fallTarget.y)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            Vector3 nextPos = Vector3.MoveTowards(
                _rb.position,
                fallTarget,
                fallSpeed * Time.deltaTime
            );

            _rb.MovePosition(nextPos);
        }

        UseGravity = true;
        IsGrounded = true;
        gameObject.layer = 10;
    }

    private List<Transform> SpawnRocksAroundBoss()
    {
        List<Transform> rocks = new List<Transform>();

        int amount = 3;

        for (int i = 0; i < amount; i++)
        {
            float angle = i * (360f / amount);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * _rockSpawnRadius, -5f, Mathf.Sin(angle * Mathf.Deg2Rad) * _rockSpawnRadius);

            GameObject rock = Instantiate(_rocksHide, transform.position + offset, Quaternion.identity);
            rocks.Add(rock.transform);

            _activeRocks.Add(rock);
        }

        return rocks;
    }

    private IEnumerator RaiseRocksRoutine(List<Transform> rocks)
    {
        float timer = 0f;
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();

        foreach (var r in rocks)
        {
            Vector3 start = r.position;
            Vector3 end = start + Vector3.up * _rockRiseHeight;

            startPositions.Add(start);
            endPositions.Add(end);
        }

        while (timer < _rockRiseDuration)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            float t = timer / _rockRiseDuration;

            for (int i = 0; i < rocks.Count; i++)
            {
                rocks[i].position = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }

            timer += Time.deltaTime;
        }

        for (int i = 0; i < rocks.Count; i++)
        {
            rocks[i].position = endPositions[i];
        }
    }

    private void DestroyRocks(List<Transform> rocks)
    {
        GameObject x = null;
        foreach (var r in rocks)
        {
            if (_rocksDestroyParticles != null)
            {
                x = Instantiate(_rocksDestroyParticles, r.position, Quaternion.identity);
            }
            x.GetComponent<ParticleSystem>().Play();
            Destroy(r.gameObject);

            _activeRocks.Remove(r.gameObject);
        }
    }

    private void DestroyActiveRocks()
    {
        if (_activeRocks == null || _activeRocks.Count == 0) return;

        foreach (var rock in _activeRocks.ToArray())
        {
            if (rock == null) continue;
            if (_rocksDestroyParticles != null)
            {
                Instantiate(_rocksDestroyParticles, rock.transform.position, Quaternion.identity);
            }
            Destroy(rock);
        }
        _activeRocks.Clear();
    }
    #endregion

    #region MultiRayThrow
     public void StartMultiRayCombo()
     {
         if (_performingRaySequence || _isDashing || _isPerformingMultiRayCombo || Stuned)
         {
             return;
         }
         ResetBossState();
         PrepareImpulse();
         if (_multiRayCoroutine != null) StopCoroutine(_multiRayCoroutine);
         _multiRayCoroutine = StartCoroutine(MultiRayComboRoutine());
     }
    /*public void StartMultiRayCombo()
    {
        if (_bossBusy) return;

        _bossBusy = true;
        ResetBossState();
        PrepareImpulse();
        if (_multiRayCoroutine != null) StopCoroutine(_multiRayCoroutine);
        _multiRayCoroutine = StartCoroutine(MultiRayComboRoutine());
    }*/
    private IEnumerator MultiRayComboRoutine()
    {
        _isPerformingMultiRayCombo = true;
        _moveActivate = false;
        _rotationActivate = false;
        _performingRaySequence = true;

        Transform initialPoint = PickRandomDashPoint();

        yield return RotateTowardsPoint(initialPoint.position);
        Impulse();
        yield return MoveToPoint(initialPoint.position);
        DashFin();
        for (int i = 0; i < 4; i++)
        {
            yield return ChargeAndShootRay(i);
        }
        _shootRay = false;
        _isPerformingMultiRayCombo = false;
        _performingRaySequence = false;
        Idle(1);
        OnMove(Vector3.zero);
        _multiRayCoroutine = null;
    }
    /*private IEnumerator MultiRayComboRoutine()
    {
        _isPerformingMultiRayCombo = true;
        _moveActivate = false;
        _rotationActivate = false;
        _performingRaySequence = true;

        Transform initialPoint = PickRandomDashPoint();

        yield return RotateTowardsPoint(initialPoint.position);
        Impulse();
        yield return MoveToPoint(initialPoint.position);
        DashFin();

        for (int i = 0; i < 4; i++)
        {
            yield return ChargeAndShootRay(i);
        }

        FinishMultiRayCombo();
    }*/
    /*private void FinishMultiRayCombo()
    {
        _isPerformingMultiRayCombo = false;
        _performingRaySequence = false;
        _shootRay = false;

        Idle(1);
        OnMove(Vector3.zero);

        _multiRayCoroutine = null;

        ResetBossState();

        _bossBusy = false;
    }*/
    private IEnumerator MoveToPoint(Vector3 point)
    {
        Vector3 targetPos = point;
        targetPos.y = transform.position.y;

        int safety = 0;

        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            Vector3 diff = targetPos - transform.position;
            diff.y = 0f;

            float dist = diff.magnitude;

            if (dist <= 0.3f)
            {
                _rb.MovePosition(targetPos);
                yield break;
            }

            if (safety > 300)
            {
                yield break;
            }

            safety++;
            diff.Normalize();

            float frameMove = _dashForce * Time.fixedDeltaTime;

            if (frameMove >= dist)
            {
                _rb.MovePosition(targetPos);
                yield break;
            }

            _rb.MovePosition(_rb.position + diff * frameMove);

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator RotateTowardsPoint(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            float angle = Quaternion.Angle(_rb.rotation, targetRot);

            if (angle < 4f)
                yield break;

            _rb.MoveRotation(
                Quaternion.Slerp(
                    _rb.rotation,
                    targetRot,
                    _multiRayRotateForce * Time.deltaTime
                )
            );
        }
    }

    private IEnumerator ChargeAndShootRay(int index)
    {
        ChargeRay(index + 1);

        Transform throwPoint = _rayThrowPoints[index];
        var mat = throwPoint.GetComponent<RayMatCharge>();
        if (mat != null) mat.Active();

        while (!_shootRay)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            Vector3 dir = (_tgPos - transform.position);
            dir.y = 0f;

            RotateTowardsDuringMultiRay(dir);
        }

        if (mat != null) mat.Reinicio();

        ShootRay(index + 1);

        if (_rayPrefab != null)
        {
            RayShoot ray = Instantiate(_rayPrefab, throwPoint.position, throwPoint.rotation).GetComponent<RayShoot>();
            _spawnedRays.Add(ray);
            _activatedRays.Add(ray);

            if (_tgNoPredict != null)
                ray.GetTg(_tgNoPredict.transform.position);
        }

        _shootRay = false;

        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            break;
        }
    }

    private void RotateTowardsDuringMultiRay(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        _rb.MoveRotation(
            Quaternion.Slerp(
                _rb.rotation,
                targetRot,
                _multiRayRotateForce * Time.deltaTime
            )
        );
    }

    private Transform PickRandomDashPoint()
    {
        return _rayDashPoints[Random.Range(0, _rayDashPoints.Length)];
    }
    #endregion

    #region DashRegion
    public void DashShoot()
    {
        if (_isDashing || _isRotatingToDash || Stuned)
        {
            return;
        }
        ResetBossState();
        StopAllComboCoroutines();

        _performingRaySequence = false;
        _waitingToShootRay = false;
        _shootRay = false;
        _isDashing = false;
        _isRotatingToDash = false;
        _dashCounter = 0;
        _currentDashTarget = null;

        Idle(0);
        _isDashingRoutine = true;
        PrepareImpulse();

        PickInitialDashPoint();

        Debug.Log("[DemonBoss] DashShoot requested - starting rotation to dash target: " +
                  (_currentDashTarget != null ? _currentDashTarget.name : "NULL"));
    }

    private void PickInitialDashPoint()
    {
        _currentDashTarget = _rayDashPoints[Random.Range(0, _rayDashPoints.Length)];
        StartDashRotation();
    }

    private void PickNextDashPoint()
    {
        Vector3 myPos = transform.position;

        var farthest3 = _rayDashPoints.OrderByDescending(t => Vector3.Distance(myPos, t.position)).Take(3).ToArray();

        _currentDashTarget = farthest3[Random.Range(0, farthest3.Length)];

        StartDashRotation();
    }

    private void StartDashRotation()
    {
        _isRotatingToDash = true;
        _isDashing = false;
        _rotationActivate = false;
        _moveActivate = false;
        PrepareImpulse();
    }

    private void FixedUpdateDash()
    {
        if (_performingRaySequence || _waitingToShootRay)
        {
            return;
        }

        if (_isRotatingToDash)
        {
            RotateTowardsDashTarget();
            return;
        }

        if (_isDashing)
        {
            DashMovement();
            return;
        }
    }

    private void RotateTowardsDashTarget()
    {
        if (_currentDashTarget == null)
        {
            _isRotatingToDash = false;
            return;
        }

        Vector3 dir = (_currentDashTarget.position - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, _rotationForce / 2 * Time.fixedDeltaTime));

        if (Quaternion.Angle(_rb.rotation, targetRot) < _rotationAngleThreshold)
        {
            _isRotatingToDash = false;
            StartDash();
        }
    }

    private void RotateDuringCharge()
    {
        Vector3 dir = (_tgPos - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, _rotationForce * Time.fixedDeltaTime));
    }

    private void StartDash()
    {
        Impulse();
        _isDashing = true;
        //if (_dashTrail != null) _dashTrail.Play();
    }

    private void DashMovement()
    {
        if (_currentDashTarget == null)
        {
            return;
        }
        Vector3 targetPos = _currentDashTarget.position;
        targetPos.y = transform.position.y;

        Vector3 dir = (targetPos - transform.position);
        float dist = dir.magnitude;

        if (dist < 0.05f)
        {
            _rb.MovePosition(targetPos);
            FinishDash();
            return;
        }

        dir.Normalize();

        float moveStep = _dashForce * Time.fixedDeltaTime;

        if (_dashCounter > 0 && _dashCounter < 4)
        {
            CheckDashStun(_rb.position, targetPos);
        }
        if (moveStep >= dist)
        {
            _rb.MovePosition(targetPos);
            FinishDash();
            return;
        }

        _rb.MovePosition(_rb.position + dir * moveStep);
    }

    private void FinishDash()
    {
        DashFin();
        _isDashing = false;

        if (_rayBeforeNextDashCoroutine != null)
        {
            StopCoroutine(_rayBeforeNextDashCoroutine);
            _rayBeforeNextDashCoroutine = null;
        }
        _rayBeforeNextDashCoroutine = StartCoroutine(RayBeforeNextDashRoutine());
    }

    private IEnumerator RayBeforeNextDashRoutine()
    {
        if (_dashCounter > 0)
        {
            _waitingToShootRay = true;
            _rotationActivate = false;
            _moveActivate = false;

            var mat = _rayThrowPoints[0].GetComponent<RayMatCharge>();
            if (mat != null) mat.Active();
            ChargeRay(1);

            while (!_shootRay)
            {
                yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

                RotateDuringCharge();

                yield return null;
            }

            _waitingToShootRay = false;
            if (mat != null) mat.Reinicio();

            if (_rayPrefab != null)
            {
                RayShoot spawnedRay = Instantiate(_rayPrefab, _rayThrowPoints[0].position, _rayThrowPoints[0].rotation).GetComponent<RayShoot>();

                _spawnedRays.Add(spawnedRay);
                _activatedRays.Add(spawnedRay);

                if (_tgNoPredict != null)
                    spawnedRay.GetTg(_tgNoPredict.transform.position);
            }

            _shootRay = false;
            yield return new WaitForSeconds(1f);
        }

        _performingRaySequence = false;
        _dashCounter++;

        if (_dashCounter >= 4)
        {
            Idle(1);
            _isRotatingToDash = false;
            _isDashing = false;
            _currentDashTarget = null;
            _rayBeforeNextDashCoroutine = null;
            _isRotatingToDash = false;
            ResetBossState();
            yield break;
        }

        PickNextDashPoint();
        _rayBeforeNextDashCoroutine = null;
    }

    public void SpereActive()
    {
        _shootRay = true;
    }

    private void CheckDashStun(Vector3 startPos, Vector3 endPos)
    {
        Vector3 center = (startPos + endPos) * 0.5f;
        Vector3 halfSize = _boxHalfExtents;

        Vector3 direction = (endPos - startPos);
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return;
        }
        Quaternion orientation = Quaternion.LookRotation(direction.normalized);

        Collider[] cols = Physics.OverlapBox(center, halfSize, orientation, _enemyLayer);
        foreach (Collider col in cols)
        {
            Entity entity = col.GetComponent<Entity>();

            if (entity == null || entity.Kind == Kind)
            {
                continue;
            }
            if (!entity.IsRayStunable)
            {
                continue;
            }
            if (!entity.IsGrounded)
            {
                continue;
            }
            EventManager.Ejecute(EventManager.KindOfEvent.PauseOneEnemy, col.gameObject, _stunDuration);
        }
    }
    #endregion

    #region GenericRegion
    public override void FlyFunct()
    {
        if (Life <= 0 || _isShieldCharge) return;
        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnGoinAir);
        GetToTheAir();
    }

    public override void GetToTheGround()
    {
        if (Life <= 0 || _isShieldCharge) return;
        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnGoinGround);
        GetToGround();
    }

    private void MantainOnAir()
    {
        if (Life <= 0 || _isShieldCharge) return;
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        GravValue = 0;
        _rb.AddForce(Vector3.up * 280, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        if (_groundDetect.point != null)
        {
            if (Vector3.Distance(transform.position, _groundDetect.point) <= GroundDistanceDetector)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_groundDetect.point, 0.3f);
            }
        }
    }

    private void ResetBossState()
    {
        _isDashing = false;
        _isRotatingToDash = false;
        _waitingToShootRay = false;
        _performingRaySequence = false;
        _fistToFistCombo = false;
        _shootRay = false;
        _currentDashTarget = null;
        _dashCounter = 0;
        _isDashingRoutine = false;
        StopAllComboCoroutines();

        _isDoingCloseCombo = false;
        _closeAttackCounter = 0;

        _rb.angularVelocity = Vector3.zero;

        if (_rayBeforeNextDashCoroutine != null)
        {
            StopCoroutine(_rayBeforeNextDashCoroutine);
            _rayBeforeNextDashCoroutine = null;
        }
    }

    private void StopAllComboCoroutines()
    {
        if (_closeComboCoroutine != null)
        {
            StopCoroutine(_closeComboCoroutine);
            _closeComboCoroutine = null;
        }
        if (_explosionCoroutine != null)
        {
            StopCoroutine(_explosionCoroutine);
            _explosionCoroutine = null;
            _isPerformingExplosion = false;
        }
        if (_multiRayCoroutine != null)
        {
            StopCoroutine(_multiRayCoroutine);
            _multiRayCoroutine = null;
            _isPerformingMultiRayCombo = false;
            _performingRaySequence = false;
        }
        if (_rayBeforeNextDashCoroutine != null)
        {
            StopCoroutine(_rayBeforeNextDashCoroutine);
            _rayBeforeNextDashCoroutine = null;
        }

        _waitingToShootRay = false;
        _performingRaySequence = false;
        _shootRay = false;
    }
    #endregion

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
    }
}
