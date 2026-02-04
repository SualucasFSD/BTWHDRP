using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
[RequireComponent(typeof(Rigidbody))]
public class PjModel : Entity, Idamageable
{
    [Header("Materials")]
    [SerializeField] private Material _damageBorders;
    private Coroutine _damageRoutine;
    [Header("Mov Test")]
    [Header("Variables Test")]
    public CameraManager Camera;
    public bool ManualMovement = true;
    public bool OnAttacking = false;
    [SerializeField] private ParticleSystem _bloodVfx;
    [SerializeField] private ParticleSystem _healtVfx;
    [Header("Configuracion Player")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _maxLife;
    [SerializeField] private float _velocity;
    [SerializeField] float JumpForce;
    [SerializeField][Range(1, 10)] private int _maxJumps = 2;
    [SerializeField] float DodgeForce;
    [SerializeField][Range(0, 15)] private float _gravityForce;
    [SerializeField][Range(0.2f, 4)] private float _movSpeedMultiplier;
    [Header("Cosas Varias")]
    [SerializeField] FadeinOut _fade;
    [SerializeField] private EsqeletonPower _powerSkeleton;
    [SerializeField] AcquireAbility _myAbilityText;
    [SerializeField] private float _maxAirTime;
    [SerializeField] private ParticleSystem _deathParticle;
    [SerializeField] private GameObject _bnMat;
    [SerializeField] private CustomPassVolume _bnVolume;
    private Material _matBn;
    //[SerializeField] private GameObject _deathCustomPass;
    [SerializeField] private Material[] _matPlayerRender = new Material[4];
    [SerializeField] private float _materialLerpDuration = 3;
    public bool IsDodging = false;
    public float RotationSpeedMultiply = 1;
    //Privates
    //public bool IsDashAttacking = false;
    private bool _limitZone = false;
    public float _delayGrav;
    public float _airTime;
    private Vector3 _dodgeDir;
    private float _jumpTimerReset = 0;
    private int _actualJumps = 0;
    //private bool _dodgeHasDirecction=false;
    private Dictionary<EnemyCatalogue, Tuple<int, IPjPower>> _powerActivate = new Dictionary<EnemyCatalogue, Tuple<int, IPjPower>>();
    private Collider _ownCollider;
    private bool _dodgeReset = true;
    private float _delayActions = 0;
    //AutoRotate Area
    [SerializeField] private LayerMask _enemyLayer;
    private bool _haveCloseEnemy = false;
    private GameObject _closeEnemy;
    public float RotationSpeedMultiplyNoLock = 1;
    private float _gold;
    private float _diamond;
    //[SerializeField] private float _autoRotateRadius = 6f;
    #region Eventos
    public event Action<Vector3, bool> OnMovement = delegate { };
    public event Action<Vector3, bool> OnDirectionalMovement = delegate { };
    public event Action<float> OnLifeUpdate = delegate { };
    public event Action OnDodge = delegate { };
    public event Action OnJump = delegate { };
    public event Action OnDeath = delegate { };
    public event Action<float, float> OnAim = delegate { };
    public event Action OnLockCamera = delegate { };
    public event Action<int> OnChangeLockTarget = delegate { };
    public event Action<bool> OnSprint = delegate { };
    public event Action OnAttack = delegate { };
    public event Action OnAttackLong = delegate { };
    public event Action OnAttackSecond = delegate { };
    public event Action OnAttackSecondLong = delegate { };
    public event Action OnAttackAir = delegate { };
    public event Action OnAttackLongAir = delegate { };
    public event Action OnAttackSecondAir = delegate { };
    public event Action OnAttackSecondLongAir = delegate { };
    public event Action OnCancelAction = delegate { };
    public event Action EjecutePower = delegate { };
    public event Action<float> OnFall = delegate { };
    public event Action OnLanding = delegate { };
    public event Action OnRunAttack = delegate { };
    //public event Action OnDeath=delegate{};
    #endregion
    private void Awake()
    {
        _damageBorders.SetFloat("_Vignette_radius", 0);
        Kind = KindOfEntity.Allies;
        _rb = GetComponent<Rigidbody>();
        _ownCollider = GetComponent<Collider>();
    }
    private void Start()
    {
        GravValue = _gravityForce;
        if (CameraManager.Instance != null)
        {
            Camera = CameraManager.Instance;
        }
        Life = _maxLife;
        GameManager.Instance.AddEntity(this, Kind);
        EventManager.Suscribe(EventManager.KindOfEvent.OnEnemyKilled, EnemyKilled);
        EventManager.Suscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Suscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
        if (SaveSystemManager.instance != null)
        {
            foreach (var i in SaveSystemManager.instance._saveDatas[0].PowerObtained)
            {
                _powerActivate[i].Item2.Active();
                _powerActivate.Remove(i);
            }
            _gold = SaveSystemManager.instance.GenericSave.Gold;
            _diamond = SaveSystemManager.instance.GenericSave.Diamond;
            print(_gold);
            print(_diamond);
        }
        foreach(var j in _matPlayerRender)
        {
            j.SetFloat("_Clip", 0);
        }

        foreach (var pass in _bnVolume.customPasses)
        {
            if (pass is FullScreenCustomPass fsPass)
            {
                _matBn = fsPass.fullscreenPassMaterial;
                break;
            }
        }
        _matBn.SetFloat("_BN_activate", 0f);
    }
    private void Update()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.OnPjChangePosition, transform.position, Dir);
        if (GameManager.Instance.IsPaused)
        {
            return;
        }
        if (!IsGrounded)
        {
            OnFall(_rb.linearVelocity.y);
            if (_actualJumps == 0) { _actualJumps = 1; }
        }
        else
        {
            GravValue = _gravityForce;
            _jumpTimerReset += Time.deltaTime;
            OnLanding();
            if (_jumpTimerReset > 0.5f)
            {
                _dodgeReset = true;
                _actualJumps = 0;
                _jumpTimerReset = 0;
            }
        }
        if (!Ready)
        {
            return;
        }
        if (IsDodging)
        {
            gameObject.layer = 18;
        }
        else
        {
            gameObject.layer = 11;
        }
        if (_delayActions < 1)
        {
            _delayActions += Time.deltaTime;
        }
        if (_delayGrav <= 0.8f)
        {
            _delayGrav += Time.deltaTime;
        }

        if (GravValue <= _gravityForce && _delayGrav > 0.75f)
        {
            GravValue += Time.deltaTime * 12f;
        }

        _limitZone = _groundDetect.collider != null && _groundDetect.collider.gameObject.layer == 19;

        EjecutePower();
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }
        IsGroundedDetector();

        if (_delayGrav > 0.75)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(GravValue, 2), ForceMode.Acceleration);
        }
       /* if (!IsGrounded)
        {
            OnFall(_rb.linearVelocity.y);
            if (_actualJumps == 0) { _actualJumps = 1; }
        }
        else
        {
            GravValue = _gravityForce;
            _jumpTimerReset += Time.deltaTime;
            OnLanding();
            if (_jumpTimerReset > 0.5f)
            {
                _dodgeReset = true;
                _actualJumps = 0;
                _jumpTimerReset = 0;
            }
        }*/
        if (!Ready)
        {
            return;
        }
        RotateTowardsDir();
        if (IsGrounded && Dir == Vector3.zero && _rb.linearVelocity.magnitude < 0.1f)
        {
            _rb.linearVelocity = Vector3.zero;
        }
        if (OnAttacking || IsDodging)
        { return; }
        if (Camera._focusing && Dir != Vector3.zero)
        {
            Vector3 velocityChange = (Dir * _velocity) - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(velocityChange * 50 / 1.8f, ForceMode.Acceleration);
            return;
        }
        if (Dir != Vector3.zero)
        {
            Vector3 velocityChange = (Dir * _velocity) - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(velocityChange * 50, ForceMode.Acceleration);
        }
    }
    #region Movimiento y Rotacion
    public void Movement(Vector3 rawDir, bool running)
    {
        if (Camera == null) return;
        if (rawDir.sqrMagnitude > 0f)
        {
            _dodgeDir = rawDir;
            //_dodgeHasDirecction=true;
            Vector3 camForward = Camera.gameObject.transform.forward;
            Vector3 camRight = Camera.gameObject.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            rawDir.Normalize();
            Dir = camForward * rawDir.z + camRight * rawDir.x;

            //if (OnAttacking) return;
            OnMovement(Dir, running);
        }
        else
        {
            _dodgeDir = Vector3.zero;
            //_dodgeHasDirecction = false;
            Dir = Vector3.zero;
            //if (OnAttacking) return;
            OnMovement(Dir, running);
        }
    }
    #region AutoRotate Target
    public void GetCloseEnemyPj()
    {
            if (!OnAttacking || Camera == null || Camera._focusing)
            {
                _haveCloseEnemy = false;
                _closeEnemy = null;
                return;
            }

            List<Entity> targets = GameManager.Instance.RefreshEnemy(Kind);
            if (targets == null || targets.Count == 0)
            {
                _haveCloseEnemy = false;
                _closeEnemy = null;
                return;
            }

            GameObject bestTarget = null;
            float bestDist = float.MaxValue;

            foreach (var targetEntity in targets)
            {
                if (targetEntity == null || targetEntity.gameObject == gameObject) continue;

                GameObject enemy = targetEntity.gameObject;

                if (!GameManager.Instance.LineOfSight(transform.position, enemy.transform.position))
                    continue;

                Vector3 dir = (enemy.transform.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);

                if (Physics.Raycast(transform.position + Vector3.up * 1f, dir, out RaycastHit hit, dist + 1f, _enemyLayer))
                {
                    float realDist = hit.distance;

                    if (realDist <= 5f && realDist < bestDist)
                    {
                        bestDist = realDist;
                        bestTarget = enemy;
                    }
                }
            }

            if (bestTarget != null)
            {
                _closeEnemy = bestTarget;
                _haveCloseEnemy = true;
            }
            else
            {
                _closeEnemy = null;
                _haveCloseEnemy = false;
            }
    }
    private void FallowEnemy()
    {
        if (_closeEnemy == null) return;

        Vector3 dir = _closeEnemy.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.001f) return;

        Quaternion target = Quaternion.LookRotation(dir.normalized);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, target, _rotationSpeed*RotationSpeedMultiplyNoLock * Time.fixedDeltaTime));
    }
    #endregion
    /// <summary>
    /// Calculo de rotacion y rotacion justamente XD
    /// </summary>
    private void RotateTowardsDir()
    {
        if (Camera == null) return;

        GetCloseEnemyPj();

        if (Camera._focusing) return;

        if (_haveCloseEnemy && !Camera._focusing && OnAttacking)
        {
            FallowEnemy();
            return;
        }

        if (Dir == Vector3.zero)
        {
            return;
        }
        Quaternion target = Quaternion.LookRotation(Dir);
        float rotSpeed = _rotationSpeed * RotationSpeedMultiply;
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, target, rotSpeed * Time.fixedDeltaTime));
    }
    public void AutoMove(Vector3 dir)
    {
        if (dir.magnitude == 0)
        {

            Dir = Vector3.zero;
            return;
        }
        Dir = Vector3.ClampMagnitude(Dir + dir, _velocity);
        OnMovement(dir, true);
    }
    #endregion
    #region Jump
    public void Jump()
    {
        //if (IsDashAttacking) { return; }
        if (IsGrounded || _actualJumps < _maxJumps && !IsDodging)
        {
            if (!_limitZone)
            {
                if (_delayActions > 0.1f)
                {
                    _delayActions = 0;
                    _actualJumps++;
                    OnJump();
                }
            }
        }
    }
    public void JumpExecute(params object[] p)
    {
        _rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
    }
    #endregion
    #region Dodge
    public void Dodge()
    {
        //if (IsDashAttacking) { return; }
        if (_dodgeDir.sqrMagnitude > 0.01f && !IsDodging && _dodgeReset)
        {
            if (_delayActions > 0.1f)
            {
                _delayActions = 0;
                IsDodging = true;
                _dodgeReset = false;
                OnDodge();
            }
        }
    }
    public void DodgeExecute(params object[] p)
    {
        Vector3 worldDodge = Dir.normalized;

        if (worldDodge.sqrMagnitude < 0.01f)
        {
            worldDodge = transform.forward;
        }
        if (_groundDetect.collider != null)
        {
            worldDodge = Vector3.ProjectOnPlane(worldDodge, _groundDetect.normal).normalized;
        }

        _rb.AddForce(worldDodge * DodgeForce, ForceMode.VelocityChange);
    }
    #endregion
    #region ComboKeys
    public void AttackFirstCombo()
    {
        if (OnAttack != null && !_limitZone)
        {
            if (!IsDodging)
            {
                StopMove();

                if (!IsGrounded)
                {
                    OnAttackAir();
                    return;
                }
                OnAttack();
            }
            else if (IsDodging && IsGrounded)
            {
                OnRunAttack();
            }
        }
    }
  
    public void AttackSecondCombo()
    {
        if (OnAttackSecond != null && !IsDodging && !_limitZone)
        {
            StopMove();
          
            if (!IsGrounded)
            {
                OnAttackSecondAir();
                return;
            }
            OnAttackSecond();
        }
    }
    public void AttackSecondComboLong()
    {
        if (OnAttackSecondLong != null && !IsDodging && !_limitZone)
        {
            StopMove();
          
            if (!IsGrounded)
            {
                OnAttackSecondLongAir();
                return;
            }
            OnAttackSecondLong();
        }
    }
    public void AttackFirstComboLong()
    {
        if (OnAttackLong != null && !IsDodging && !_limitZone)
        {
            StopMove();
         
            if (!IsGrounded)
            {
                OnAttackLongAir();
                return;
            }
            OnAttackLong();
        }
    }
    private void StopMove()
    {
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
        }
    }
    #endregion
    #region Camera
    public void LoockOnCamera()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.OnLockCamera);
        if (OnLockCamera != null)
        {
            OnLockCamera();
        }
    }
    public void ChangeTarget(int x)
    {
        EventManager.Ejecute(EventManager.KindOfEvent.OnChangeTarget, x);
        if (OnChangeLockTarget != null)
        {
            OnChangeLockTarget(x);
        }
    }
    public void RotateCamera(float X, float Y)
    {
        OnAim(X, Y);
    }
    public void RotatePlayer(float X, float Y)
    {
        transform.rotation = Quaternion.Euler(0, X, 0);
        OnAim(X, Y);
    }
    #endregion
    #region Genericos
    public void TakeDamage(float dmg, float exp, Vector3 pushDirection, bool downHit = false, bool airHit = false, bool isStunDamage = false, float pushForce = 1000)
    {
        if (pushDirection != Vector3.zero)
        {
            _rb.AddForce(pushDirection * 12000, ForceMode.Impulse);
        }
        Life -= dmg;
        EventManager.Ejecute(EventManager.KindOfEvent.MakeCameraShake);
        if (_bloodVfx != null)
        {
            _bloodVfx.Play();
        }
        if (_damageRoutine != null)
        {
            StopCoroutine(_damageRoutine);
        }
        _damageBorders.SetFloat("_Vignette_radius", 1);
        StartCoroutine(DamageBorders());
        OnLifeUpdate(Life / _maxLife);
        if (Life < 0)
        {
            Life = 0;
        }
        EventManager.Ejecute(EventManager.KindOfEvent.LifeUpdater, Life / _maxLife);
        if (Life <= 0)
        {
            GameManager.Instance.RemoveEntity(this, Kind);
            gameObject.layer = 18;
            EventManager.Ejecute(EventManager.KindOfEvent.OnDeath);
            /* if(_deathCustomPass!=null)
             {
                 _deathCustomPass.SetActive(true);
             }*/
            StartCoroutine(BnFade());
            if (SaveSystemManager.instance != null)
            {
                SaveSystemManager.instance._saveDatas[0]=new DataSave();
            }
            OnDeath();

            StartCoroutine(PostDeadThings());
        }
    }
    IEnumerator PostDeadThings()
    {
        yield return new WaitForSeconds(.5f);

        float time = 0f;
        if (_deathParticle != null)
        {
            _deathParticle.Play();
        }
        while (time < _materialLerpDuration)
        {
            time += Time.deltaTime;
            for (int i = 0; i < _matPlayerRender.Length; i++)
            {
                if (_matPlayerRender[i] != null)
                {
                    _matPlayerRender[i].SetFloat("_Clip", Mathf.Lerp(0, 8f, time / _materialLerpDuration));
                }
            }

            yield return null;
        }
        _fade.StartFadeIn();
        yield return new WaitForSeconds(3);


        if (_materialLerpDuration <= 0f)
        {
            _materialLerpDuration = 3;
        }
        for (int i = 0; i < _matPlayerRender.Length; i++)
        {
            if (_matPlayerRender[i] != null)
            {
                _matPlayerRender[i].SetFloat("_Clip", 0);
            }
        }
        EventManager.Ejecute(EventManager.KindOfEvent.ResetLevel, "Hub");
    }
    public void TakeHealt(float amount)
    {
        Life += amount;
        if (_healtVfx != null)
        {
            _healtVfx.Play();
        }
        if (Life > _maxLife)
        {
            EventManager.Ejecute(EventManager.KindOfEvent.MaxLifeReach);
            Life = _maxLife;
        }
        EventManager.Ejecute(EventManager.KindOfEvent.LifeUpdater, Life / _maxLife);
    }
    public void AddPower(EnemyCatalogue Obj, Tuple<int, IPjPower> Needed)
    {
        if (!_powerActivate.ContainsKey(Obj))
        {
            _powerActivate.Add(Obj, Needed);
        }
    }
    private IEnumerator DamageBorders()
    {
        float i = 1;
        while (i >= 0)
        {
            i -= 0.1f;
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            _damageBorders.SetFloat("_Vignette_radius", i);
            yield return new WaitForSeconds(0.1f);
        }
        _damageRoutine = null;
    }
    public void EnemyKilled(object[] obj)
    {
        if (!_powerActivate.ContainsKey((EnemyCatalogue)obj[1]))
        {
            return;
        }
        _powerActivate[(EnemyCatalogue)obj[1]] = Tuple.Create(_powerActivate[(EnemyCatalogue)obj[1]].Item1 - 1, _powerActivate[(EnemyCatalogue)obj[1]].Item2);

        if (_powerActivate[(EnemyCatalogue)obj[1]].Item1 <= 0)
        {
            /*if (!_myAbilityText.isPlaying)
                _myAbilityText.StartCoroutine(_myAbilityText.OnAbilityAcquired());*/
            if(SaveSystemManager.instance!=null)
            {
                SaveSystemManager.instance._saveDatas[0].PowerObtained.Add((EnemyCatalogue)obj[1]);
            }
            _powerActivate[(EnemyCatalogue)obj[1]].Item2.Active();
            _powerActivate.Remove((EnemyCatalogue)obj[1]);
        }
        else
        {
            print("Faltan " + _powerActivate[(EnemyCatalogue)obj[1]].Item1 + " " + (EnemyCatalogue)obj[1]);
        }
    }
    public void ComboInitial()
    {
        _rb.linearVelocity = Vector3.zero;
        RotationSpeedMultiply = 0.2f;
    }
    public void DesactiveGravity()
    {
        _delayGrav = 0;
        GravValue = 0;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
    }
    public void StopPJ()
    {
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
        }
    }
    public void ActiveGravity()
    {
        GravValue = _gravityForce;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    #endregion
    public void GetDown()
    {
        GravValue = _gravityForce;
        _delayGrav = 2;
        _rb.AddForce(-Vector3.up * 5000, ForceMode.Impulse);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction: -Vector3.up * 10);
        if (_groundDetect.point != null)
        {
            if (Vector3.Distance(transform.position, _groundDetect.point) <= GroundDistanceDetector)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_groundDetect.point, 0.3f);
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Dir.normalized * 0.3f, 0.5f);
    }
    IEnumerator BnFade()
    {
        if (_bnVolume == null || _matBn == null)
            yield break;

        _bnMat.SetActive(true);
        _bnVolume.enabled = true;

        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float value = Mathf.Lerp(0f, 1f, elapsed / duration);
            _matBn.SetFloat("_BN_activate", value);

            yield return null;
        }

        _matBn.SetFloat("_BN_activate", 1f);
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnEnemyKilled, EnemyKilled);
        EventManager.Unscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Unscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
    }
}
