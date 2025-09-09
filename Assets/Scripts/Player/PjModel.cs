using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Rigidbody))]
public class PjModel : Entity, Idamageable
{
    [Header("Variables Test")]
    public CameraManager Camera;
    public bool ManualMovement = true;
    //public bool OnAnimation = false, OnJumpAnim = false;
    public bool OnAttacking=false;
    [Header("Configuracion Player")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _maxLife;
    [SerializeField] private float _velocity;
    [SerializeField] float JumpForce;
    [SerializeField][Range(1,10)] private int _maxJumps=2;
    [SerializeField] float DodgeForce;
    [SerializeField][Range(0, 10)] private float _rayDistance;
    [SerializeField][Range(0, 15)] private float _gravityForce;
    [SerializeField][Range(0.2f, 4)] private float _movSpeedMultiplier;
    [SerializeField] private LayerMask _stopLayer;
    [Header("Cosas Varias")]
    [SerializeField] private Image _lifeBar;
    [SerializeField] private EsqeletonPower _powerSkeleton;
    [SerializeField] AcquireAbility _myAbilityText;
    private Vector3 _dodgeDir;
    private int _actualJumps=0;
    private float _jumpTimerReset=0;
    private bool _useGravity=true;
    private RaycastHit _groundHit;
    private float _pathTimer=0;
    private Dictionary<EnemyCatalogue, Tuple<int, IPjPower>> _powerActivate = new Dictionary<EnemyCatalogue, Tuple<int, IPjPower>>();
    private bool _isGrounded=false;
    #region Eventos
    public event Action<Vector3,bool> OnMovement = delegate { };
    public event Action<Vector3, bool> OnDirectionalMovement = delegate { };
    public event Action<float> OnLifeUpdate=delegate { };
    public event Action<Vector3> OnDodge = delegate { };
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
    public event Action OnCancelAction = delegate { };
    public event Action EjecutePower=delegate { };
    public event Action<float> OnFall=delegate { };
    public event Action OnLanding = delegate { };
    #endregion
    private void Awake()
    {
        Kind=KindOfEntity.Allies;
        _rb = GetComponent<Rigidbody>();
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
        EventManager.Suscribe(EventManager.KindOfEvent.KnightComboReset, AttackAnimReset);
        EventManager.Suscribe(EventManager.KindOfEvent.KnightJumpReset, JumpReset);
        EventManager.Suscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Suscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
    }
    private void Update()
    {
        EjecutePower();
        if (!Camera._focusing&&Dir!=Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(Dir),_rotationSpeed * Time.deltaTime);
        }
    }
    private void FixedUpdate()
    {
        if (_useGravity)
        {
           _rb.AddForce(-transform.up * Mathf.Pow(_gravityForce, 1.7f), ForceMode.Acceleration);
        }
        if (!Physics.Raycast(transform.position, -transform.up, maxDistance: 1.5f, hitInfo: out _groundHit))
        {
            _isGrounded = false;
            if (OnFall != null)
            {
                OnFall(_rb.velocity.y);
            }
            if(_actualJumps==0)
            {
                _actualJumps = 1;
            }
        }
        else
        {
            _jumpTimerReset += Time.deltaTime;
            _isGrounded = true;
            if (OnLanding != null)
            {
                OnLanding();
            }
            if(_jumpTimerReset>0.5f)
            {
                _actualJumps = 0;
            }
        }
        if (Physics.SphereCast(transform.position, 0.4f, Dir.normalized, out RaycastHit p, 0.3f, _stopLayer))
        {
            return;
        }
        if (Dir != Vector3.zero&&!OnAttacking)
        {
            _rb.MovePosition(_rb.position + Dir * _velocity * Time.fixedDeltaTime);
        }
    }
    public void Movement(Vector3 dir, Vector3 rawDir, bool running)
    {
        if (Camera == null)
            return;
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

            OnMovement(Dir, running);
        }
        else
        {
            _dodgeDir = Vector3.zero;
            Dir = Vector3.zero;
            OnMovement(Dir, running);
        }
    }
    public void Dodge(Vector3 dir)
    {
        if (_dodgeDir.sqrMagnitude > 0)
        {
            OnDodge(Dir);
        }
    }

    public void AutoMove(Vector3 dir)
    {
       if (dir.magnitude == 0)
       {

        Dir = Vector3.zero;
         return;
       }
        _pathTimer += Time.fixedDeltaTime;
        if(_pathTimer>3f)
        {
            EventManager.Ejecute(EventManager.KindOfEvent.ReloadPath);
            _pathTimer = 0;
        }
        Dir = Vector3.ClampMagnitude(Dir + dir,_velocity);
        OnMovement(dir,true);
    }
    public void Jump()
    {
        if(_isGrounded||_actualJumps <_maxJumps)
        {
            _actualJumps++;
            OnJump();
        }
    }
    public void AttackFirstCombo()
    {
        /*if (OnJumpAnim) return;
        OnAnimation=true;*/
        if (OnAttack != null)
        {
            OnAttack();
        }
    }
    public void AttackSecondCombo()
    {
        /*if (OnJumpAnim) return;
        OnAnimation = true;*/
        if (OnAttackSecond != null)
        {
            OnAttackSecond();
        }
    }
    public void AttackSecondComboLong()
    {
        /*if (OnJumpAnim) return;
        OnAnimation = true;*/
        if (OnAttackSecondLong != null)
        {
            OnAttackSecondLong();
        }
    }
    public void AttackFirstComboLong()
    {
        /*if (OnJumpAnim) return;
        OnAnimation = true;*/
        if (OnAttackLong != null)
        {
            OnAttackLong();
        }
    }
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
    public void TakeDamage(float dmg, float exp, Vector3 pushDirection)
    {
        if (pushDirection != Vector3.zero)
        {
            _rb.AddForce(pushDirection * 1000, ForceMode.Impulse);
        }
        Life -=dmg;
        OnLifeUpdate(Life / _maxLife);
        if (Life < 0)
        {
            Life = 0;
        }
       if (_lifeBar != null)
       {
         _lifeBar.fillAmount = Life / _maxLife;
       }
       if (Life <= 0)
       {
            GameManager.Instance.RemoveEntity(this, Kind);
            //Destroy(gameObject);
            //EventManager.ResetEvent();
            EventManager.Ejecute(EventManager.KindOfEvent.OnDeath);
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        if (_lifeBar != null)
        {
            _lifeBar.fillAmount = Life / _maxLife;
        }
    }
    public void AddPower(EnemyCatalogue Obj, Tuple<int,IPjPower> Needed)
    {
        if(!_powerActivate.ContainsKey(Obj))
        {
            
            _powerActivate.Add(Obj, Needed);
        }
    }
    #region Eventos
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
    #endregion
    #region Eventos De Animacion
    public void JumpExecute(params object[] p) { _rb.AddForce(transform.up * JumpForce, ForceMode.Impulse); /*OnAnimation = false;*/ }
    public void DodgeExecute(params object[] p)
    {
        Vector3 inputDir = new Vector3(_dodgeDir.x, 0f, _dodgeDir.z);
        if (inputDir.sqrMagnitude < 0.01f)
        {
            inputDir = Vector3.forward;
        }
        Vector3 camForward = Camera.transform.forward;
        Vector3 camRight = Camera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dodgeDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

        if (_groundHit.collider != null)
        {
            dodgeDir = Vector3.ProjectOnPlane(dodgeDir, _groundHit.normal).normalized;
        }
        _rb.AddForce(dodgeDir * DodgeForce, ForceMode.Impulse);
    }

    public void AttackAnimReset(params object[] p)
    {
        //OnAnimation = false;
    }
    public void JumpReset(params object[] p)
    {
        //OnJumpAnim = false;
    }
    #endregion
    private void OnEnable()
    {
    
    }
    private void OnDisable()
    {
  
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position - transform.up + transform.up * 0.2f, -transform.up * _rayDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Dir.normalized * 0.3f, 0.5f);
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.KnightComboReset, AttackAnimReset);
        EventManager.Unscribe(EventManager.KindOfEvent.JumpPj, JumpExecute);
        EventManager.Unscribe(EventManager.KindOfEvent.KnightJumpReset, JumpReset);
        EventManager.Unscribe(EventManager.KindOfEvent.KnightExecuteDodge, DodgeExecute);
    }
}
