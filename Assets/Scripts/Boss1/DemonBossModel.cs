//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;
//using Random = UnityEngine.Random;
//[RequireComponent(typeof(Rigidbody))]
//public class DemonBossModel : Entity, Idamageable
//{
//    [Header("General Components and Values")]
//    [SerializeField][Range(0, 1000)] private float _shieldLife = 1000;
//    [SerializeField] private Rigidbody _rb;
//    [SerializeField] private LayerMask _obstacleMask;
//    [SerializeField] private ParticleSystem _bloodVfx;
//    [SerializeField] private float _rotationForce;
//    [SerializeField] private float _moveForce;
//    [SerializeField] private float _groundImpulse = 2500f;
//    [SerializeField] private Image _shieldPercent;
//    [SerializeField] private ParticleSystem _shieldBrokeEffect;
//    [SerializeField] private Transform _centerPoint;
//    [SerializeField] private GameObject _rocksHide;
//    [SerializeField] private ParticleSystem _rocksDestroyParticles;
//    [SerializeField] private float _rockSpawnRadius = 6f;
//    [SerializeField] private float _rockRiseHeight = 4f;
//    [SerializeField] private float _rockRiseDuration = 1.5f;
//    [SerializeField] private float _explosionChargeTime = 3f;
//    [SerializeField] private GameObject _bigThunderWave;
//    [Header("Ray Throw")]
//    [SerializeField] private GameObject _rayPrefab;
//    [SerializeField] private Transform[] _rayThrowPoints = new Transform[4];
//    [SerializeField] private Transform[] _rayDashPoints = new Transform[16];
//    private bool _waitingToShootRay = false;
//    private bool _performingRaySequence = false;

//    [Header("Dash")]
//    [SerializeField] private float _dashForce;
//    [SerializeField] private ParticleSystem _impulseParticle;
//    [SerializeField] private ParticleSystem _dashTrail;
//    private Transform _currentDashTarget;
//    private int _dashCounter = 0;
//    private bool _isRotatingToDash = false;
//    private bool _isDashing = false;
//    private float _rotationAngleThreshold = 4f;
//    [SerializeField] private LayerMask _enemyLayer;
//    [SerializeField] private float _stunDuration = 3f;
//    [SerializeField] private Vector3 _boxHalfExtents = new Vector3(2f, 2f, 2f);
//    [Header("ExplosionCharge")]
//    [SerializeField] private float _chargeDuration;
//    [SerializeField] private ParticleSystem _chargeParticle;
//    [SerializeField] private ParticleSystem _explosionParticle;
//    private float _explosionIti = 0;
//    private float _areaDamage;
//    //PRIVATES GENERIC
//    [SerializeField] private float _multiRayRotateForce = 8f;
//    private bool _isPerformingMultiRayCombo = false;
//    private bool _rotationActivate = false;
//    private bool _moveActivate = false;
//    private bool _isShieldCharge = true;
//    private float _predictionTime = 0.5f;
//    private Vector3 _lastPos;
//    private Vector3 _tgPos;
//    private FsmDemonBoss _fsm = new FsmDemonBoss();
//    private GameObject _tgNoPredict;
//    private bool _isPerformingExplosion = false;
//    //EVENTOS
//    public event Action PrepareImpulse = delegate { };
//    public event Action Impulse = delegate { };
//    public event Action DashFin = delegate { };
//    public event Action<bool> Grounded = delegate { };
//    public event Action<Vector3> OnMove = delegate { };
//    public event Action GetToTheAir = delegate { };
//    public event Action GetToGround = delegate { };
//    public event Action OnAirHit = delegate { };
//    public event Action OnHitStunt = delegate { };

//    private void Awake()
//    {
//        IsRayStunable = false;
//        if (_rb == null)
//        {
//            _rb = GetComponent<Rigidbody>();
//        }
//        _rb.useGravity = false;
//    }

//    private void Start()
//    {
//        GameManager.Instance.AddEntity(this, KindOfEntity.Enemy);
//        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].Life;
//        EventManager.Suscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);

//        _fsm.AddState(FsmDemonBoss.AgentStates.OnGoinAir, new OnGoingAirState(this, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnMidAir), 4.5f, GroundDistanceDetector, _rb));
//        _fsm.AddState(FsmDemonBoss.AgentStates.OnMidAir, new OnAir(this, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat)));
//        _fsm.AddState(FsmDemonBoss.AgentStates.OnGoinGround, new ToTheGroundState(this, _rb, _groundImpulse, EnemyCatalogue.DemonBoss, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat)));
//        _fsm.AddState(FsmDemonBoss.AgentStates.OnCombat, new DashStateDemonBoos());

//        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat);
//        Invoke(nameof(InvokeDash), 5);
//    }
//    private void InvokeDash()
//    {
//        StartExplosionCombo();
//        //DashShoot();
//    }

//    private void Update()
//    {
//        _tgNoPredict = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(Kind), transform);
//        if (GameManager.Instance.IsPaused)
//            return;

//        if (UseGravity)
//        {
//            if (GravValue < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].GravityForce)
//                GravValue += Time.deltaTime * 7f;
//        }

//        _fsm.ArtificialUpdate();
//    }

//    private void FixedUpdate()
//    {
//        if (GameManager.Instance.IsPaused)
//            return;

//        _fsm.ArtificialFixedUpdate();
//        IsGroundedDetector();

//        if (UseGravity)
//        {
//            _rb.AddForce(-transform.up * Mathf.Pow(GravValue, 2), ForceMode.Acceleration);
//        }
//        if (Stuned || !_isShieldCharge)
//        { return; }

//        if (_moveActivate)
//        { OnMovePj(); }

//        if (_rotationActivate)
//        { RotateToTarget(_tgNoPredict.transform.position); }

//        FixedUpdateDash();
//    }
//#region Move and Rotate Region
//    public void OnMovePj()
//    {
//        if (_tgPos == Vector3.zero || Stuned)
//        {
//            Dir = Vector3.zero;
//            OnMove(Vector3.zero);
//            return;
//        }

//        if (Vector3.Distance(transform.position, _tgPos) < 1.5f)
//        {
//            return;
//        }
//        OnMove(Dir);
//        Dir = (_tgPos - transform.position).normalized;
//        _rb.MovePosition(_rb.position + new Vector3(Dir.x, 0, Dir.z) * _moveForce * Time.fixedDeltaTime);
//    }

//    private void RotateToTarget(Vector3 Direction)
//    {
//        if (_rb == null)
//        {
//            return;
//        }
//        Direction -= transform.position;
//        Direction.y = 0f;
//        if (Direction.sqrMagnitude < 0.0001f)
//        { return; }

//        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(Direction.normalized, Vector3.up), _rotationForce * Time.fixedDeltaTime));
//    }

//    private void AreaDamage()
//    {
//        EventManager.Ejecute(EventManager.KindOfEvent.MakeCameraShake);
//        var p = GameManager.Instance.RefreshEnemy(Kind);
//        foreach (var r in p)
//        {
//            if(!GameManager.Instance.SphereLineOfSight(_tgPos,transform.position,0.5f))
//            { continue; }
//            if (Vector3.Distance(transform.position - Vector3.up * 2, r.transform.position) < 500)
//            {
//                if (r.TryGetComponent<Idamageable>(out var damageable))
//                {
//                    Vector3 pushDir = new Vector3((r.transform.position - transform.position).x, 0f, (r.transform.position - transform.position).z).normalized;
//                    damageable.TakeDamage(_areaDamage, 0, pushDir);
//                }
//            }
//        }
//    }

//    private void TakePjPosition(params object[] p)
//    {
//        Vector3 playerPos = (Vector3)p[0];

//        Vector3 playerVel = (playerPos - _lastPos) / Time.deltaTime;
//        _lastPos = playerPos;

//        Vector3 predictedPos = playerPos;
//        if (playerVel.magnitude > 0.1f)
//        { predictedPos += playerVel.normalized * playerVel.magnitude * _predictionTime; }

//        Vector3 shootOrigin = transform.position;
//        Vector3 dir = (predictedPos - shootOrigin).normalized;
//        float dist = Vector3.Distance(shootOrigin, predictedPos);

//        if (Physics.Raycast(shootOrigin, dir, out RaycastHit hit, dist, _obstacleMask))
//        {
//            predictedPos = hit.point;
//        }
//        _tgPos = predictedPos;
//    }
//    #endregion
//    #region IdamageableRegion
//    public void TakeDamage(float dmg, float stunt, Vector3 pushDirection, bool downHit = false, bool airHit = false, bool isStuntDamage = false, float pushForce = 1000)
//    {
//        if (Life <= 0)
//            return;

//        if (_isShieldCharge)
//        {
//            _shieldLife -= dmg;
//            if (_shieldLife <= 0)
//            {
//                if (_shieldBrokeEffect != null)
//                    _shieldBrokeEffect.Play();

//                _isShieldCharge = false;
//                //StopAllCoroutines();
//                //ResetBossState();
//                StartCoroutine(ShieldRechardRoutine());
//            }
//        }
//        else
//        {
//            if (_bloodVfx != null && isStuntDamage)
//                _bloodVfx.Play();

//            Life -= dmg;

//            if (!downHit && isStuntDamage)
//            {
//                if (IsGrounded)
//                    OnHitStunt();
//                else
//                {
//                    OnAirHit();
//                    MantainOnAir();
//                }
//            }
//        }
//    }

//    public void TakeHealt(float amount) { }

//    IEnumerator ShieldRechardRoutine()
//    {
//        while (_shieldLife < 1000)
//        {
//            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
//            if (_shieldPercent != null)
//                _shieldPercent.fillAmount = _shieldLife / 1000;

//            _shieldLife += 10f;
//            yield return new WaitForSeconds(0.1f);
//        }
//        _isShieldCharge = true;
//    }
//    #endregion
//    #region ExplosionCombo
//    public void StartExplosionCombo()
//    {
//        if (_isPerformingExplosion)
//            return;

//        ResetBossState();
//        StartCoroutine(ExplosionComboRoutine());
//    }

//    private IEnumerator ExplosionComboRoutine()
//    {
//        _isPerformingExplosion = true;
//        _moveActivate = false;
//        _rotationActivate = false;

//        FaceCenter();

//        yield return StartCoroutine(JumpToCenterRoutine());

//        if (_bigThunderWave != null)
//        {
//            Instantiate(_bigThunderWave, transform.position, Quaternion.identity);
//        }
//        List<Transform> spawnedRocks = SpawnRocksAroundBoss();

//        yield return StartCoroutine(RaiseRocksRoutine(spawnedRocks));

//        float timer = 0f;
//        float explodeTime = 2.5f;

//        while (timer < explodeTime)
//        {
//            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

//            if (!_isShieldCharge)
//            {
//                //StunBoss();
//                DestroyRocks(spawnedRocks);
//                _isPerformingExplosion = false;
//                _moveActivate = true;
//                _rotationActivate = true;
//                yield break;
//            }

//            timer += Time.deltaTime;
//        }

//        AreaDamage();
//        DestroyRocks(spawnedRocks);

//        _isPerformingExplosion = false;
//        _moveActivate = true;
//        _rotationActivate = true;
//    }

//    private void FaceCenter()
//    {
//        Vector3 dir = _centerPoint.position - transform.position;
//        dir.y = 0;
//        if (dir.sqrMagnitude > 0.1f)
//        {
//            Quaternion target = Quaternion.LookRotation(dir.normalized);
//            transform.rotation = target;
//        }
//    }

//    private IEnumerator JumpToCenterRoutine()
//    {
//        Vector3 targetPos = _centerPoint.position;
//        Vector3 startPos = transform.position;

//        float peakHeight = startPos.y + 12f;
//        float midHeight = (startPos.y + peakHeight) * 0.5f;

//        float ascendSpeed = 45f;
//        float forwardSpeed = 35f;
//        float fallSpeed = 65f;

//        //JumpExecute();
//        UseGravity = false;
//        IsGrounded = false;
//        gameObject.layer = 18;

//        while (transform.position.y < midHeight)
//        {
//            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

//            Vector3 dir = (targetPos - transform.position);
//            dir.y = 1f;
//            dir.Normalize();

//            transform.position += dir * ascendSpeed * Time.deltaTime;
//        }

//        while (transform.position.y < peakHeight)
//        {
//            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

//            Vector3 dir = (targetPos - transform.position);
//            dir.y = 0.3f;
//            dir.Normalize();

//            transform.position += dir * forwardSpeed * Time.deltaTime;
//        }

//        yield return new WaitForSeconds(0.25f);
//        //MaxHeigh();

//        Vector3 fallTarget = new Vector3(targetPos.x, startPos.y, targetPos.z);

//        while (transform.position.y > fallTarget.y)
//        {
//            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

//            transform.position = Vector3.MoveTowards(transform.position,fallTarget,fallSpeed * Time.deltaTime);
//        }

//        UseGravity = true;
//        IsGrounded = true;
//        gameObject.layer = 10;
//    }

//    private List<Transform> SpawnRocksAroundBoss()
//    {
//        List<Transform> rocks = new List<Transform>();

//        int amount = 3;

//        for (int i = 0; i < amount; i++)
//        {
//            float angle = i * (360f / amount);
//            Vector3 offset = new Vector3( Mathf.Cos(angle * Mathf.Deg2Rad) *_rockSpawnRadius,-5f,Mathf.Sin(angle * Mathf.Deg2Rad) *_rockSpawnRadius);

//            GameObject rock = Instantiate(_rocksHide, transform.position + offset, Quaternion.identity);
//            rocks.Add(rock.transform);
//        }

//        return rocks;
//    }


//    private IEnumerator RaiseRocksRoutine(List<Transform> rocks)
//    {
//        float duration = 1.5f;
//        float timer = 0f;

//        while (timer < duration)
//        {
//            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

//            foreach (var r in rocks)
//                r.position += Vector3.up * 2f * Time.deltaTime;

//            timer += Time.deltaTime;
//        }
//    }
//    private void DestroyRocks(List<Transform> rocks)
//    {
//        foreach (var r in rocks)
//        {
//            if (_rocksDestroyParticles != null)
//            {
//                Instantiate(_rocksDestroyParticles, r.position, Quaternion.identity);
//            }
//            Destroy(r.gameObject);
//        }
//    }
//    #endregion
//    #region MultiRayThrow
//    public void StartMultiRayCombo()
//    {
//        if (_performingRaySequence || _isDashing || _isPerformingMultiRayCombo)
//        {
//            return;
//        }
//        ResetBossState();
//        StartCoroutine(MultiRayComboRoutine());
//    }
//    private IEnumerator MultiRayComboRoutine()
//    {
//        _isPerformingMultiRayCombo = true;
//        _moveActivate = false;
//        _rotationActivate = false;

//        Transform initialPoint = PickRandomDashPoint();

//        yield return RotateTowardsPoint(initialPoint.position);

//        yield return MoveToPoint(initialPoint.position);

//        for (int i = 0; i < 4; i++)
//        {
//            yield return ChargeAndShootRay(i);
//        }

//        _isPerformingMultiRayCombo = false;
//        _moveActivate = true;
//        _rotationActivate = true;
//    }
//    private IEnumerator MoveToPoint(Vector3 point)
//    {
//        Vector3 targetPos = point;
//        targetPos.y = transform.position.y;

//        int safety = 0;

//        while (true)
//        {
//            Vector3 diff = targetPos - transform.position;
//            diff.y = 0f;

//            float dist = diff.magnitude;

//            if (dist <= 0.3f)
//            {
//                _rb.MovePosition(targetPos);
//                yield break;
//            }

//            if (safety > 300)
//            {
//                yield break;
//            }

//            safety++;
//            diff.Normalize();

//            float frameMove = _dashForce * Time.fixedDeltaTime;

//            if (frameMove >= dist)
//            {
//                _rb.MovePosition(targetPos);
//                yield break;
//            }

//            _rb.MovePosition(_rb.position + diff * frameMove);

//            yield return new WaitForFixedUpdate();
//        }
//    }
//    private IEnumerator RotateTowardsPoint(Vector3 point)
//    {
//        Vector3 dir = point - transform.position;
//        dir.y = 0f;

//        if (dir.sqrMagnitude < 0.001f)
//            yield break;

//        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

//        while (true)
//        {
//            float angle = Quaternion.Angle(_rb.rotation, targetRot);

//            if (angle < 4f)
//                yield break;

//            _rb.MoveRotation(
//                Quaternion.Slerp(
//                    _rb.rotation,
//                    targetRot,
//                    _multiRayRotateForce * Time.deltaTime
//                )
//            );

//            yield return null;
//        }
//    }
//    private IEnumerator ChargeAndShootRay(int index)
//    {
//        Transform throwPoint = _rayThrowPoints[index];

//        RayShoot ray = Instantiate(_rayPrefab, throwPoint.position, throwPoint.rotation).GetComponent<RayShoot>();

//        ray.Active();

//        float timer = 0f;

//        while (timer < 1f)
//        {
//            timer += Time.deltaTime;

//            Vector3 dir = (_tgPos - transform.position);
//            dir.y = 0f;

//            RotateTowardsDuringMultiRay(dir);

//            yield return null;
//        }

//        ray.GetTg(_tgPos);

//        while (ray != null)
//            yield return null;
//    }
//    private void RotateTowardsDuringMultiRay(Vector3 dir)
//    {
//        if (dir.sqrMagnitude < 0.001f)
//            return;

//        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
//        _rb.MoveRotation(
//            Quaternion.Slerp(
//                _rb.rotation,
//                targetRot,
//                _multiRayRotateForce * Time.deltaTime
//            )
//        );
//    }
//    private Transform PickRandomDashPoint()
//    {
//        return _rayDashPoints[Random.Range(0, _rayDashPoints.Length)];
//    }
//    #endregion
//    #region DashRegion
//    public void DashShoot()
//    {
//        if (_isDashing || _isRotatingToDash)
//        {
//            return;
//        }
//        ResetBossState();
//        _dashCounter = 0;
//        PickInitialDashPoint();
//    }

//    private void PickInitialDashPoint()
//    {
//        _currentDashTarget = _rayDashPoints[UnityEngine.Random.Range(0, _rayDashPoints.Length)];
//        StartDashRotation();
//    }

//    private void PickNextDashPoint()
//    {
//        Vector3 myPos = transform.position;

//        var farthest3 = _rayDashPoints.OrderByDescending(t => Vector3.Distance(myPos, t.position)).Take(3).ToArray();

//        _currentDashTarget = farthest3[UnityEngine.Random.Range(0, farthest3.Length)];

//        StartDashRotation();
//    }

//    private void StartDashRotation()
//    {
//        _isRotatingToDash = true;
//        _isDashing = false;
//        _rotationActivate = false;
//        _moveActivate = false;
//        PrepareImpulse();
//    }

//    private void FixedUpdateDash()
//    {
//        if (_performingRaySequence || _waitingToShootRay)
//            return;

//        if (_isRotatingToDash)
//        {
//            RotateTowardsDashTarget();
//            return;
//        }

//        if (_isDashing)
//        {
//            DashMovement();
//            return;
//        }
//    }

//    private void RotateTowardsDashTarget()
//    {
//        if (_currentDashTarget == null)
//        {
//            _isRotatingToDash = false;
//            return;
//        }

//        Vector3 dir = (_currentDashTarget.position - transform.position);
//        dir.y = 0f;

//        if (dir.sqrMagnitude < 0.01f)
//            return;

//        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
//        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, _rotationForce/2 * Time.fixedDeltaTime));

//        if (Quaternion.Angle(_rb.rotation, targetRot) < _rotationAngleThreshold)
//        {
//            _isRotatingToDash = false;
//            StartDash();
//        }
//    }

//    private void StartDash()
//    {
//        Impulse();
//        _isDashing = true;
//        if (_dashTrail != null) _dashTrail.Play();
//    }

//    private void DashMovement()
//    {
//        if (_currentDashTarget == null)
//            return;

//        Vector3 targetPos = _currentDashTarget.position;
//        targetPos.y = transform.position.y;

//        Vector3 dir = (targetPos - transform.position);
//        float dist = dir.magnitude;

//        if (dist < 0.05f)
//        {
//            _rb.MovePosition(targetPos);
//            FinishDash();
//            return;
//        }

//        dir.Normalize();

//        float moveStep = _dashForce * Time.fixedDeltaTime;

//        if (_dashCounter > 0 && _dashCounter < 4)
//        {
//            CheckDashStun(_rb.position, targetPos);
//        }
//        if (moveStep >= dist)
//        {
//            _rb.MovePosition(targetPos);
//            FinishDash();
//            return;
//        }

//        _rb.MovePosition(_rb.position + dir * moveStep);
//    }
//    private void FinishDash()
//    {
//        DashFin();
//        _isDashing = false;
//        StartCoroutine(RayBeforeNextDashRoutine());
//    }
//    private IEnumerator RayBeforeNextDashRoutine()
//    {
//        if (_dashCounter > 0)
//        {
//            _waitingToShootRay = true;
//        _rotationActivate = false;
//        _moveActivate = false;

//        while (true)
//        {
//            Vector3 dir = (_tgPos - transform.position);
//            dir.y = 0f;

//            RotateToTarget(dir);

//            float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));
//            if (angle < 5f) break;

//            yield return null;
//        }

//        _waitingToShootRay = false;

//        RayShoot spawnedRay = Instantiate(_rayPrefab, _rayThrowPoints[0].position, transform.rotation).GetComponent<RayShoot>();
//        spawnedRay.Active();
//        _performingRaySequence = true;

//        yield return new WaitForSeconds(1f);

//        spawnedRay.GetTg(_tgPos);

//        yield return new WaitForSeconds(1f);
//        }

//        _performingRaySequence = false;
//        _dashCounter++;

//        //Finalizando Dash
//        if (_dashCounter >= 4)
//        {
//            _moveActivate = true;
//            _rotationActivate = true;
//            _isRotatingToDash = false;
//            _isDashing = false;
//            _currentDashTarget = null;
//            StartMultiRayCombo();
//            yield break;
//        }

//        PickNextDashPoint();
//    }
//    private void CheckDashStun(Vector3 startPos, Vector3 endPos)
//    {
//        Vector3 center = (startPos + endPos) * 0.5f;
//        Vector3 halfSize = _boxHalfExtents;

//        Vector3 direction = (endPos - startPos);
//        float distance = direction.magnitude;

//        if (distance <= 0.01f)
//        {
//            return;
//        }
//        Quaternion orientation = Quaternion.LookRotation(direction.normalized);

//        Collider[] cols = Physics.OverlapBox(center, halfSize, orientation, _enemyLayer);
//        print("DashDamage");
//        foreach (Collider col in cols)
//        {
//            Entity entity = col.GetComponent<Entity>();

//            if (entity == null || entity.Kind == Kind)
//            {
//                continue;
//            }
//            if (!entity.IsRayStunable)
//            {
//                continue;
//            }
//            if(!entity.IsGrounded)
//            {
//                continue;
//            }
//            EventManager.Ejecute(EventManager.KindOfEvent.PauseOneEnemy, col.gameObject, _stunDuration);
//        }
//    }
//    #endregion
//    #region GenericRegion
//    public override void FlyFunct()
//    {
//        if (Life <= 0 || _isShieldCharge) return;
//        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnGoinAir);
//        GetToTheAir();
//    }

//    public override void GetToTheGround()
//    {
//        if (Life <= 0 || _isShieldCharge) return;
//        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnGoinGround);
//        GetToGround();
//    }

//    private void MantainOnAir()
//    {
//        if (Life <= 0 || _isShieldCharge) return;
//        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
//        GravValue = 0;
//        _rb.AddForce(Vector3.up * 280, ForceMode.Impulse);
//    }

//    private void OnDrawGizmos()
//    {
//        if (_groundDetect.point != null)
//        {
//            if (Vector3.Distance(transform.position, _groundDetect.point) <= GroundDistanceDetector)
//            {
//                Gizmos.color = Color.red;
//                Gizmos.DrawSphere(_groundDetect.point, 0.3f);
//            }
//        }
//    }
//    private void ResetBossState()
//    {
//        _isDashing = false;
//        _isRotatingToDash = false;
//        _waitingToShootRay = false;
//        _performingRaySequence = false;
//        _moveActivate = false;
//        _rotationActivate = false;

//        _currentDashTarget = null;
//        _dashCounter = 0;

//        _rb.angularVelocity = Vector3.zero;
//    }
//    #endregion

//    private void OnDestroy()
//    {
//        EventManager.Unscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
//    }
//}
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
    [Header("General Components and Values")]
    [SerializeField][Range(0, 1000)] private float _shieldLife = 1000;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private ParticleSystem _bloodVfx;
    [SerializeField] private float _rotationForce;
    [SerializeField] private float _moveForce;
    [SerializeField] private float _groundImpulse = 2500f;
    [SerializeField] private Image _shieldPercent;
    [SerializeField] private ParticleSystem _shieldBrokeEffect;
    [SerializeField] private Transform _centerPoint;
    [SerializeField] private GameObject _rocksHide;
    [SerializeField] private ParticleSystem _rocksDestroyParticles;
    [SerializeField] private float _rockSpawnRadius = 6f;
    [SerializeField] private float _rockRiseHeight = 4f;
    [SerializeField] private float _rockRiseDuration = 1.5f;
    [SerializeField] private float _explosionChargeTime = 3f;
    [SerializeField] private GameObject _bigThunderWave;
    [Header("Ray Throw")]
    [SerializeField] private GameObject _rayPrefab;
    [SerializeField] private Transform[] _rayThrowPoints = new Transform[4];
    [SerializeField] private Transform[] _rayDashPoints = new Transform[16];
    private bool _waitingToShootRay = false;
    private bool _performingRaySequence = false;

    [Header("Dash")]
    [SerializeField] private float _dashForce;
    [SerializeField] private ParticleSystem _impulseParticle;
    [SerializeField] private ParticleSystem _dashTrail;
    private Transform _currentDashTarget;
    private int _dashCounter = 0;
    private bool _isRotatingToDash = false;
    private bool _isDashing = false;
    private float _rotationAngleThreshold = 4f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _stunDuration = 3f;
    [SerializeField] private Vector3 _boxHalfExtents = new Vector3(2f, 2f, 2f);
    [Header("ExplosionCharge")]
    [SerializeField] private float _chargeDuration;
    [SerializeField] private ParticleSystem _chargeParticle;
    [SerializeField] private ParticleSystem _explosionParticle;
    [SerializeField] private float _areaDamage;
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
    private bool _shootRay=false;
    //private bool _isStunned = false;

    //EVENTOS
    public event Action PrepareImpulse = delegate { };
    public event Action Impulse = delegate { };
    public event Action DashFin = delegate { };
    public event Action<int> Idle=delegate { };
    public event Action<int> ChargeRay=delegate { };
    public event Action<int> ShootRay=delegate { };
    public event Action<bool> Grounded = delegate { };
    public event Action<Vector3> OnMove = delegate { };
    public event Action GetToTheAir = delegate { };
    public event Action GetToGround = delegate { };
    public event Action OnAirHit = delegate { };
    public event Action OnHitStunt = delegate { };
    public event Action OnAttackClose=delegate { };

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
        Life = GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].Life;
        EventManager.Suscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);

        _fsm.AddState(FsmDemonBoss.AgentStates.OnGoinAir, new OnGoingAirState(this, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnMidAir), 4.5f, GroundDistanceDetector, _rb));
        _fsm.AddState(FsmDemonBoss.AgentStates.OnMidAir, new OnAir(this, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat)));
        _fsm.AddState(FsmDemonBoss.AgentStates.OnGoinGround, new ToTheGroundState(this, _rb, _groundImpulse, EnemyCatalogue.DemonBoss, () => _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat)));
        _fsm.AddState(FsmDemonBoss.AgentStates.OnCombat, new DashStateDemonBoos());

        _fsm.ChangeState(FsmDemonBoss.AgentStates.OnCombat);
    }

    private void Update()
    {
        _tgNoPredict = GameManager.Instance.GetCloseEnemy(GameManager.Instance.RefreshEnemy(Kind), transform);
        if (GameManager.Instance.IsPaused)
            return;

        if (UseGravity)
        {
            if (GravValue < GameManager.Instance.EnemyConfiguration[EnemyCatalogue.DemonBoss].GravityForce)
                GravValue += Time.deltaTime * 7f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartExplosionCombo();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DashShoot();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartMultiRayCombo();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCloseAttackCombo();
        }
        _fsm.ArtificialUpdate();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused)
            return;

        _fsm.ArtificialFixedUpdate();
        IsGroundedDetector();

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

        /*if (_rotationActivate)
        { RotateToTarget(_tgNoPredict.transform.position); }*/

        FixedUpdateDash();
    }

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
        var p = GameManager.Instance.RefreshEnemy(Kind);
        foreach (var r in p)
        {
            if (!GameManager.Instance.SphereLineOfSight(_tgPos, transform.position, 0.5f))
            { continue; }
            if (Vector3.Distance(transform.position - Vector3.up * 2, r.transform.position) < 500)
            {
                if (r.TryGetComponent<Idamageable>(out var damageable))
                {
                    Vector3 pushDir = new Vector3((r.transform.position - transform.position).x, 0f, (r.transform.position - transform.position).z).normalized;
                    damageable.TakeDamage(_areaDamage, 0, pushDir);
                }
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
            return;
        ResetBossState();
        StartCoroutine(CloseAttackCombo());
    }

    private IEnumerator CloseAttackCombo()
    {
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

            if (!_isShieldCharge || Stuned)
            {
                ResetCloseCombo();
                yield break;
            }

            Vector3 toPlayer = _tgPos - transform.position;
            float distance = toPlayer.magnitude;

            if (_rotationActivate)
            {
                RotateToTarget(_tgPos);
            }

            if (distance <= hitDistance)
            {
                StopMove();
                StopRotate();

                OnAttackClose();
                print("Punches");

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
                continue;
            }

            walkTimer += Time.deltaTime;

            if (walkTimer >= maxWalkTime || distance >= dashDistance)
            {
                Impulse();
                yield return DashToPlayer(dashOffset);

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

        Vector3 dir = (_tgNoPredict.transform.position - transform.position).normalized;
        Vector3 target = _tgNoPredict.transform.position - dir * offset;
        target.y = transform.position.y;

        while (true)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            if (!_isShieldCharge || Stuned)
                yield break;

            Vector3 current = transform.position;
            Vector3 toTarget = target - current;

            if (toTarget.sqrMagnitude <= 0.2f)
            {
                _rb.MovePosition(target);
                _rb.angularVelocity = Vector3.zero;
                yield break;
            }

            RotateToTarget(_tgNoPredict.transform.position);

            Vector3 next = Vector3.MoveTowards(
                current,
                target,
                dashSpeed * Time.deltaTime
            );

            _rb.MovePosition(next);
        }
    }

    private void ResetCloseCombo(bool normalEnd = false)
    {
        //StopMove();
        StopRotate();
        _isDoingCloseCombo = false;

        if (normalEnd)
            _closeAttackCounter = 0;
        else
            _closeAttackCounter = 0;

        _moveActivate = false;
        _rotationActivate = false;
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
            return;

        if (Stuned)
        {
            if (_bloodVfx != null && isStuntDamage)
                _bloodVfx.Play();

            Life -= dmg;

            if (!downHit && isStuntDamage)
            {
                if (IsGrounded)
                    OnHitStunt();
                else
                {
                    OnAirHit();
                    MantainOnAir();
                }
            }
            return;
        }

        if (_isShieldCharge)
        {
            _shieldLife -= dmg;
            if (_shieldLife <= 0)
            {
                if (_shieldBrokeEffect != null)
                    _shieldBrokeEffect.Play();

                _isShieldCharge = false;

                StopAllCoroutines();
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

                OnHitStunt();

                //_isStunned = true;
                Stuned = true;

                StartCoroutine(ShieldRechardRoutine());
            }
        }
        else
        {
            if (_bloodVfx != null && isStuntDamage)
                _bloodVfx.Play();

            Life -= dmg;

            if (!downHit && isStuntDamage)
            {
                if (IsGrounded)
                    OnHitStunt();
                else
                {
                    OnAirHit();
                    MantainOnAir();
                }
            }
        }
    }

    public void TakeHealt(float amount) { }

    IEnumerator ShieldRechardRoutine()
    {
        while (_shieldLife < 1000)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            if (_shieldPercent != null)
                _shieldPercent.fillAmount = _shieldLife / 1000;

            _shieldLife += 10f;
            yield return new WaitForSeconds(0.1f);
        }
        _isShieldCharge = true;
        Stuned = false;
        ResetBossState();
       // _moveActivate = true;
       // _rotationActivate = true;
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
            return;

        ResetBossState();
        StartCoroutine(ExplosionComboRoutine());
    }

    private IEnumerator ExplosionComboRoutine()
    {
        _isPerformingExplosion = true;
        _moveActivate = false;
        _rotationActivate = false;

        FaceCenter();

        yield return StartCoroutine(JumpToCenterRoutine());

        if (_bigThunderWave != null)
        {
            Instantiate(_bigThunderWave, transform.position, Quaternion.identity);
        }
        List<Transform> spawnedRocks = SpawnRocksAroundBoss();

        yield return StartCoroutine(RaiseRocksRoutine(spawnedRocks));

        float timer = 0f;
        

        while (timer < _explosionChargeTime)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            if (!_isShieldCharge)
            {
                DestroyRocks(spawnedRocks);
                _isPerformingExplosion = false;
                //_moveActivate = true;
                //_rotationActivate = true;
                yield break;
            }

            timer += Time.deltaTime;
        }

        AreaDamage();
        DestroyRocks(spawnedRocks);

        _isPerformingExplosion = false;
        //_moveActivate = true;
        //_rotationActivate = true;
    }

    private void FaceCenter()
    {
        Vector3 dir = _centerPoint.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(dir.normalized);
            transform.rotation = target;
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

        //JumpExecute();
        UseGravity = false;
        IsGrounded = false;
        gameObject.layer = 18;

        while (_rb.position.y < peakHeight)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            Vector3 horizontal = targetPos - _rb.position;
            horizontal.y = 0;
            horizontal.Normalize();

            Vector3 finalDir = (horizontal * 0.2f + Vector3.up * 1f).normalized;

            _rb.MovePosition(_rb.position + finalDir * ascendSpeed * Time.deltaTime);
        }

        yield return new WaitForSeconds(0.25f);
        //MaxHeigh();

        Vector3 fallTarget = new Vector3(targetPos.x, startPos.y, targetPos.z);

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
        foreach (var r in rocks)
        {
            if (_rocksDestroyParticles != null)
            {
                Instantiate(_rocksDestroyParticles, r.position, Quaternion.identity);
            }
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
        if (_performingRaySequence || _isDashing || _isPerformingMultiRayCombo)
        {
            return;
        }
        ResetBossState();
        PrepareImpulse();
        StartCoroutine(MultiRayComboRoutine());
    }
    private IEnumerator MultiRayComboRoutine()
    {
        _isPerformingMultiRayCombo = true;
        _moveActivate = false;
        _rotationActivate = false;

        Transform initialPoint = PickRandomDashPoint();

        yield return RotateTowardsPoint(initialPoint.position);
        Impulse();
        yield return MoveToPoint(initialPoint.position);
        DashFin();
        for (int i = 0; i < 4; i++)
        {
            yield return ChargeAndShootRay(i);
        }

        _isPerformingMultiRayCombo = false;
        Idle(1);
        //_moveActivate = true;
        //_rotationActivate = true;
    }
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
        ChargeRay(index);

        Transform throwPoint = _rayThrowPoints[index];

        RayShoot ray = Instantiate(_rayPrefab, throwPoint.position, throwPoint.rotation).GetComponent<RayShoot>();

        _spawnedRays.Add(ray);

        ray.Active();

        float timer = 0f;

        while (timer < 1f)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

            timer += Time.deltaTime;

            Vector3 dir = (_tgPos - transform.position);
            dir.y = 0f;

            RotateTowardsDuringMultiRay(dir);
        }

        _activatedRays.Add(ray);
        ShootRay(index);
        ray.GetTg(_tgPos);

        while (ray != null)
        {
            if (GameManager.Instance.IsPaused)
            {
                yield return null;
                continue;
            }
            yield return null;
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
        if (_isDashing || _isRotatingToDash)
        {
            return;
        }
        ResetBossState();
        _dashCounter = 0;
        PickInitialDashPoint();
    }

    private void PickInitialDashPoint()
    {
        _currentDashTarget = _rayDashPoints[UnityEngine.Random.Range(0, _rayDashPoints.Length)];
        StartDashRotation();
    }

    private void PickNextDashPoint()
    {
        Vector3 myPos = transform.position;

        var farthest3 = _rayDashPoints.OrderByDescending(t => Vector3.Distance(myPos, t.position)).Take(3).ToArray();

        _currentDashTarget = farthest3[UnityEngine.Random.Range(0, farthest3.Length)];

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

    private void StartDash()
    {
        Impulse();
        _isDashing = true;
        if (_dashTrail != null) _dashTrail.Play();
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
        StartCoroutine(RayBeforeNextDashRoutine());
    }
    private IEnumerator RayBeforeNextDashRoutine()
    {
        if (_dashCounter > 0)
        {
            _waitingToShootRay = true;
            _rotationActivate = false;
            _moveActivate = false;

            while (true)
            {
                yield return new WaitUntil(() => !GameManager.Instance.IsPaused);

                Vector3 dir = (_tgPos - transform.position);
                dir.y = 0f;

                RotateToTarget(dir);

                float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));
                if (angle < 5f) break;
            }

            _waitingToShootRay = false;

            RayShoot spawnedRay = Instantiate(_rayPrefab, _rayThrowPoints[0].position, transform.rotation).GetComponent<RayShoot>();
            //RayShoot spawnedRay = Instantiate(_rayPrefab,_rayThrowPoints[0].position,Quaternion.identity, _rayThrowPoints[0]).GetComponent<RayShoot>();
            spawnedRay.transform.parent = _rayThrowPoints[0];
            _spawnedRays.Add(spawnedRay);

            spawnedRay.Active();
            ChargeRay(1);
            _performingRaySequence = true;

            yield return new WaitUntil(() => _shootRay==true);

            _activatedRays.Add(spawnedRay);

            spawnedRay.GetTg(_tgPos);

            _shootRay = false;
            yield return new WaitForSeconds(1f);
        }

        _performingRaySequence = false;
        _dashCounter++;

        //Finalizando Dash
        if (_dashCounter >= 4)
        {
            //_moveActivate = true;
            //_rotationActivate = true;
            Idle(1);
            _isRotatingToDash = false;
            _isDashing = false;
            _currentDashTarget = null;
            //StartMultiRayCombo();
            yield break;
        }

        PickNextDashPoint();
    }
    public void SpereActive()
    {
        _shootRay=true;
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
        //print("DashDamage");
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
        //_moveActivate = false;
        //_rotationActivate = false;
        _shootRay=false;
        _currentDashTarget = null;
        _dashCounter = 0;

        _isDoingCloseCombo = false;
        _closeAttackCounter = 0;

        _rb.angularVelocity = Vector3.zero;
    }
    #endregion

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnPjChangePosition, TakePjPosition);
    }
}
