using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
[RequireComponent(typeof(Rigidbody))]
public class PjModel : Entity, Idamageable
{
    [Header("Mov Test")]
    [SerializeField] PhysicMaterial _movMaterial;
    [SerializeField] PhysicMaterial _standMaterial;
    [Header("Variables Test")]
    public CameraManager Camera;
    public bool ManualMovement = true; 
    public bool OnAttacking=false;
    [SerializeField] private VisualEffect _bloodVfx;
    [Header("Configuracion Player")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _maxLife;
    [SerializeField] private float _velocity;
    [SerializeField] float JumpForce;
    [SerializeField][Range(1,10)] private int _maxJumps=2;
    [SerializeField] float DodgeForce;
    [SerializeField][Range(0, 15)] private float _gravityForce;
    [SerializeField][Range(0.2f, 4)] private float _movSpeedMultiplier;
    [Header("Cosas Varias")]
    [SerializeField] private EsqeletonPower _powerSkeleton;
    [SerializeField] AcquireAbility _myAbilityText;
    [SerializeField] private float _maxAirTime;
    public bool IsDodging=false;
    public bool _useGravity=true;
    public float RotationSpeedMultiply=1;
    //Privates
    public float _airTime;
    private Vector3 _dodgeDir;
    private float _jumpTimerReset = 0;
    private int _actualJumps = 0;
    private float _rotationD = 15f;
    private float _alignmentEpsilon = 0.5f;
    private Dictionary<EnemyCatalogue, Tuple<int, IPjPower>> _powerActivate = new Dictionary<EnemyCatalogue, Tuple<int, IPjPower>>();
    private Collider _ownCollider;
    private bool _isStoped=false;
    private bool _dodgeReset=true;
    #region Eventos
    public event Action<Vector3,bool> OnMovement = delegate { };
    public event Action<Vector3, bool> OnDirectionalMovement = delegate { };
    public event Action<float> OnLifeUpdate=delegate { };
    public event Action OnDodge = delegate { };
    public event Action OnJump=delegate { };
    public event Action OnDeath=delegate { };
    public event Action<float,float> OnAim = delegate { };
    public event Action OnLockCamera=delegate { };
    public event Action<int> OnChangeLockTarget = delegate { };
    public event Action<bool> OnSprint=delegate { };
    public event Action OnAttack=delegate { };
    public event Action OnAttackLong = delegate { };
    public event Action OnAttackSecond = delegate { };
    public event Action OnAttackSecondLong = delegate { };
    public event Action OnAttackAir = delegate { };
    public event Action OnAttackLongAir = delegate { };
    public event Action OnAttackSecondAir = delegate { };
    public event Action OnAttackSecondLongAir = delegate { };
    public event Action OnCancelAction = delegate { };
    public event Action EjecutePower=delegate { };
    public event Action<float> OnFall=delegate { };
    public event Action OnLanding = delegate { };
    #endregion
    private void Awake()
    {
        Kind=KindOfEntity.Allies;
        _rb = GetComponent<Rigidbody>();
        _ownCollider = GetComponent<Collider>();
    }
    private void Start()
    {
        if(CameraManager.Instance!=null)
        {
            Camera=CameraManager.Instance;
        }
        Life=_maxLife;
        GameManager.Instance.AddEntity(this, Kind);
        EventManager.Suscribe(EventManager.KindOfEvent.OnEnemyKilled, EnemyKilled);
        EventManager.Suscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Suscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
    }
    private void Update()
    {
        EjecutePower();
    }
    private void FixedUpdate()
    {
        IsGroundedDetector();

        if (_useGravity)
        {
            _rb.AddForce(-transform.up * Mathf.Pow(_gravityForce,2), ForceMode.Acceleration);
        }

        if (!IsGrounded)
        {
            OnFall(_rb.velocity.y);
            if (_actualJumps == 0) { _actualJumps = 1; }
        }
        else
        {
            _jumpTimerReset += Time.deltaTime;
            OnLanding();
            if (_jumpTimerReset > 0.5f)
            {
                _dodgeReset=true;
                _actualJumps = 0;
                _jumpTimerReset = 0;
            }
        }
        RotateTowardsDir();
        if (OnAttacking||IsDodging|| _isStoped)
        { return; }
         if (Camera._focusing && Dir != Vector3.zero)
         {
            Vector3 velocityChange = (Dir * _velocity) - new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.AddForce(velocityChange * 50/1.8f, ForceMode.Acceleration);
            return;
         }
         if (Dir != Vector3.zero)
         {
            Vector3 velocityChange = (Dir*_velocity) - new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.AddForce(velocityChange * 50, ForceMode.Acceleration);
         }
    }
    #region Movimiento y Rotacion
    public void Movement(Vector3 rawDir, bool running)
    {
        if (Camera == null)
        {
            return;
        }
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
            _ownCollider.material = _movMaterial;
            Dir = camForward * rawDir.z + camRight * rawDir.x;
            if(OnAttacking)
            {
                return;
            }
            OnMovement(Dir, running);
        }
        else
        {
            _ownCollider.material=_standMaterial;
            _dodgeDir = Vector3.zero;
            Dir = Vector3.zero;
            if (OnAttacking)
            {
                return;
            }
            OnMovement(Dir, running);
        }
    }
    /// <summary>
    /// Calculo de rotacion y rotacion justamente XD
    /// </summary>
    private void RotateTowardsDir()
    {
        if (!Camera._focusing)
        {
            if (Dir == Vector3.zero)
            {
                _rb.angularVelocity = Vector3.zero;
                return;
            }

            Quaternion target = Quaternion.LookRotation(Dir);
            Quaternion delta = target * Quaternion.Inverse(_rb.rotation);

            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;

            if (Mathf.Abs(angle) < _alignmentEpsilon)
            {
                _rb.angularVelocity = Vector3.zero;
                _rb.rotation = target;
                return;
            }

            float p = _rotationSpeed * RotationSpeedMultiply;
            Vector3 torque = axis.normalized * angle * Mathf.Deg2Rad * p - _rb.angularVelocity * _rotationD;

            _rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }
    public void AutoMove(Vector3 dir)
    {
       if (dir.magnitude == 0)
       {

        Dir = Vector3.zero;
         return;
       }
        Dir = Vector3.ClampMagnitude(Dir + dir,_velocity);
        OnMovement(dir,true);
    }
    #endregion
    #region Jump
    public void Jump()
    {
        if(IsGrounded||_actualJumps <_maxJumps && !IsDodging)
        {
            _actualJumps++;
            OnJump();
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
            IsDodging = true;
            _dodgeReset = false;
            _ownCollider.material = _movMaterial;
            //_rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            OnDodge();
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
        if (OnAttack != null&&!IsDodging)
        {
            if (!IsGrounded)
            {
                OnAttackAir();
                return;
            }
            OnAttack();
        }
    }
    public void AttackSecondCombo()
    {
        if (OnAttackSecond != null && !IsDodging)
        {
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
        if (OnAttackSecondLong != null && !IsDodging)
        {
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
        if (OnAttackLong != null && !IsDodging)
        {
            if (!IsGrounded)
            {
                OnAttackLongAir();
                return;
            }
            OnAttackLong();
        }
    }
    #endregion
    #region Camera
    public void LoockOnCamera()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.OnLockCamera);
        if(OnLockCamera!=null)
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
    public void TakeDamage(float dmg, float exp, Vector3 pushDirection)
    {
        if (pushDirection != Vector3.zero)
        {
            _rb.AddForce(pushDirection * 1000, ForceMode.Impulse);
        }
        Life -=dmg;
        _bloodVfx?.Play();
        OnLifeUpdate(Life / _maxLife);
        if (Life < 0)
        {
            Life = 0;
        }
        EventManager.Ejecute(EventManager.KindOfEvent.LifeUpdater,Life / _maxLife);
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
        if(Life>_maxLife)
        {
            Life = _maxLife;
        }
        EventManager.Ejecute(EventManager.KindOfEvent.LifeUpdater, Life / _maxLife);
    }
    public void AddPower(EnemyCatalogue Obj, Tuple<int,IPjPower> Needed)
    {
        if(!_powerActivate.ContainsKey(Obj))
        {
            _powerActivate.Add(Obj, Needed);
        }
    }

    public void EnemyKilled(object[] obj)
    {
        if (!_powerActivate.ContainsKey((EnemyCatalogue)obj[1]))
        {
            return;
        }
        _powerActivate[(EnemyCatalogue)obj[1]] = Tuple.Create(_powerActivate[(EnemyCatalogue)obj[1]].Item1-1, _powerActivate[(EnemyCatalogue)obj[1]].Item2);

        if(_powerActivate[(EnemyCatalogue)obj[1]].Item1<=0)
        {
            if (!_myAbilityText.isPlaying)
                _myAbilityText.StartCoroutine(_myAbilityText.OnAbilityAcquired());
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
        _rb.velocity = Vector3.zero;
        RotationSpeedMultiply = 0.2f;
    }
    public void DesactiveGravity()
    {
        _useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX| RigidbodyConstraints.FreezeRotationZ;
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
    }
    public void StopPJ()
    {
        if (!_rb.isKinematic)
        {
            _rb.velocity = Vector3.zero;
        }
        _isStoped = true;
        Invoke(nameof(StopedInvoke), 0.5f);
    }
    private void StopedInvoke()
    {
        _isStoped = false;
    }
    public void ActiveGravity()
    {
        _useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction: -Vector3.up*10);
        if (_groundDetect.point!=null)
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
        EventManager.Unscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Unscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
    }
}
