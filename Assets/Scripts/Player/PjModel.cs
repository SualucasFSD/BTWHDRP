using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
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
    [SerializeField] private EsqeletonPower _powerSkeleton;
    [SerializeField] AcquireAbility _myAbilityText;
    [SerializeField] private float _maxAirTime;
    [SerializeField] private LayerMask _enemyLayer;
    public bool IsDodging = false;
    public float RotationSpeedMultiply = 1;
    //Privates
    private bool _limitZone = false;
    public float _delayGrav;
    public float _airTime;
    private Vector3 _dodgeDir;
    private float _jumpTimerReset = 0;
    private int _actualJumps = 0;
    private Dictionary<EnemyCatalogue, Tuple<int, IPjPower>> _powerActivate = new Dictionary<EnemyCatalogue, Tuple<int, IPjPower>>();
    private Collider _ownCollider;
    private bool _dodgeReset = true;
    private float _delayActions = 0;
    [SerializeField] private float _autoRotateRadius = 6f;
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
    }
    private void Update()
    {
        if (GameManager.Instance.IsPaused)
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
        //if (_limitZone) { Debug.LogWarning("LimitZone"); }
        EjecutePower();
        EventManager.Ejecute(EventManager.KindOfEvent.OnPjChangePosition, transform.position, Dir);
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
            Vector3 camForward = Camera.gameObject.transform.forward;
            Vector3 camRight = Camera.gameObject.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            rawDir.Normalize();
            Dir = camForward * rawDir.z + camRight * rawDir.x;

            if (OnAttacking) return;
            OnMovement(Dir, running);
        }
        else
        {
            _dodgeDir = Vector3.zero;
            Dir = Vector3.zero;
            if (OnAttacking) return;
            OnMovement(Dir, running);
        }
    }
    /// <summary>
    /// Calculo de rotacion y rotacion justamente XD
    /// </summary>
    private void RotateTowardsDir()
    {
        if (Camera._focusing) return;

        if (Dir == Vector3.zero)
            return;

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
        if (OnAttack != null && !IsDodging && !_limitZone)
        {
            StopMove();
          
            if (!IsGrounded)
            {
                OnAttackAir();
                return;
            }
            OnAttack();
        }
        /*if(OnAttack != null&&IsDodging && !_limitZone)
        {
            if (IsGrounded)
            {
                OnRunAttack();
            }
        }*/
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
    /*private Transform GetClosestEnemy(float radius, LayerMask hitLayer)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, hitLayer);
        Transform closestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider c in colliders)
        {
            if (c.gameObject == gameObject) continue;

            Entity entity = c.GetComponent<Entity>();
            if (entity == null || entity.Kind == KindOfEntity.Allies) continue;

            float verticalDiff = Mathf.Abs(c.transform.position.y - transform.position.y);
            if (verticalDiff > 2f) continue;

            float dist = Vector3.Distance(transform.position, c.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestEnemy = c.transform;
            }
        }

        return closestEnemy;
    }
    private void RotateTowardsEnemy(Transform enemy)
    {
        if (enemy == null) return;

        Vector3 direction = (enemy.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }*/
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
    public void TakeDamage(float dmg, float exp, Vector3 pushDirection, bool downHit = false)
    {
        if (pushDirection != Vector3.zero)
        {
            _rb.AddForce(pushDirection * 1000, ForceMode.Impulse);
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
            EventManager.Ejecute(EventManager.KindOfEvent.OnDeath);
            Destroy(gameObject);
        }
    }
    public void TakeHealt(float amount)
    {
        Life += amount;
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

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnEnemyKilled, EnemyKilled);
        EventManager.Unscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Unscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
    }
}
