using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using Unity.Burst.Intrinsics;

public class KnightView : PjView
{
    public List<ComboObject> Combos = new List<ComboObject>();
    private List<KindOfCombo> _currentImputs = new List<KindOfCombo>();
    HashSet<string> _combosFinish = new HashSet<string>();
    [Header("ComboManager")]
    [SerializeField] private ParticleSystem _trail;
    [SerializeField] private GameObject _swordModel;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private float _dmgMultiply = 1;
    [SerializeField] protected GameObject _swordThrowable;
    //[SerializeField] protected GameObject _swordPoint;
    private Vector3 _lastPosition;
    private Vector3 _velocity;
    private float _swordDistance;
    private float _dmg;
    private float _swordArea;
    private float _stuntDmg;
    private float _angle;
    private float _flyAngle;
    private bool _getGround;
    private bool _airHit;
    private bool _animMove=false;
    public List<GameObject> HitEnemies = new List<GameObject>();
    private float _jumpDelay=0;
    private float _rotForceOrig = 0;
    private float _pushForce;
    private bool _onDashHitDelay = false;
    private bool _isSwordDepend;
    private Renderer[] _swordRender;
    private SwordThrowableDamage _swordThrowed=null;
    private Coroutine _swordCoroutine;
    private void Start()
    {
        _swordRender = _swordModel.GetComponentsInChildren<Renderer>();
        _pjModel = GetComponentInParent<PjModel>();
        _animator = GetComponentInChildren<Animator>();
        if (_pjModel == null)
        {
            return;
        }
        EventManager.Suscribe(EventManager.KindOfEvent.RefreshEnemyHitList, RefreshEnemyList);

        _pjModel.OnAttack += () => RegisterImputs(KindOfCombo.Light);
        _pjModel.OnAttackSecond += () => RegisterImputs(KindOfCombo.Strong);
        _pjModel.OnAttackLong += () => RegisterImputs(KindOfCombo.LightLong);
        _pjModel.OnAttackSecondLong += () => RegisterImputs(KindOfCombo.StrongLong);

        _pjModel.OnAttackAir += () => RegisterImputs(KindOfCombo.LightAir);
        _pjModel.OnAttackSecondAir += () => RegisterImputs(KindOfCombo.StrongAir);
        _pjModel.OnAttackLongAir += () => RegisterImputs(KindOfCombo.LightLongAir);
        _pjModel.OnAttackSecondLongAir += () => RegisterImputs(KindOfCombo.StrongLongAir);
        _pjModel.OnRunAttack += () => RunComboLight();

        _pjModel.OnMovement += OnMove;
        _pjModel.OnJump += OnJump;
        _pjModel.OnFall += OnFall;
        _pjModel.OnLanding += OnLanding;
        _pjModel.OnDodge += OnDodge;
        _pjModel.OnDirectionalMovement += OnDirectionalMove;
        _pjModel.OnLifeUpdate += OnLifeUpdate;
        _pjModel.OnDeath += DeathAnim;
    }
    private void Update()
    {
        _velocity = (_swordModel.transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = _swordModel.transform.position;
        if(_jumpDelay<0.5f)
        {
            _jumpDelay += Time.deltaTime;
        }
    }

    private void RefreshEnemyList(params object[] p)
    {
        HitEnemies = (List < GameObject >) p[0];
    }
    #region Jump System
    private void OnFall(float fallVelocity)
    {
        _animator.SetBool("IsGrounded", false);
        _animator.SetFloat("yAxis", fallVelocity);
    }
    public void Landing()
    {
        _animator.SetBool("Jump", false);
        EventManager.Ejecute(EventManager.KindOfEvent.KnightJumpReset);
    }
    public void Falling()
    {
        _pjModel.GravValue = 0;
    }
    private void OnLanding()
    {
        _animator.SetBool("IsGrounded", true);
    }
    private void DeathAnim()
    {
        
         _animator.CrossFadeInFixedTime("WarriorDeath", 0.25f, 0, 0f);

        _animator.CrossFadeInFixedTime("WarriorDeath", 0.25f, 1, 0f);
    }
    private void OnJump()
    {
        //_animMove = false;
        _jumpDelay = 0;
        _pjModel.IsDodging = false;
        ComboResetGeneral();
        _animator.SetBool("CancelSwordDrop", false);
        _swordCoroutine = null;
        if (_swordThrowed != null)
        {
            _swordThrowed.Cancel();
            _swordThrowed = null;
        }
        _pjModel.RotationSpeedMultiplyNoLock = 1;
        _pjModel.RotationSpeedMultiply = 1;
        _animator.CrossFadeInFixedTime("JumpForce", 0.25f, 0, 0f);

        _animator.CrossFadeInFixedTime("JumpForce", 0.25f, 1, 0f);
        //_animator.SetTrigger("Jump");
        //_animator.SetBool("Jump", true);
        EventManager.Ejecute(EventManager.KindOfEvent.JumpPj);
    }
    #endregion
    #region Move System
    private void OnMove(Vector3 _dir, bool running)
    {
        //print(_dir.sqrMagnitude);
        //_animator.SetBool("DodgeHasDirecction", DodgeHasDir);
        if (_dir.sqrMagnitude > 0)
        {
            _animator.SetBool("isMoving", true);
            if (!_pjModel.IsDodging && !_pjModel.OnAttacking && _pjModel.IsGrounded&&_jumpDelay>=0.5f)
            {
                var current = _animator.GetCurrentAnimatorStateInfo(1);
                var next = _animator.GetNextAnimatorStateInfo(1);
                bool alreadyInMoveTree =current.IsName("MoveTree") ||next.IsName("MoveTree") ||_animator.IsInTransition(1);

                if (!alreadyInMoveTree)
                {
                    print("ForceMove");
                    _animator.CrossFadeInFixedTime("MoveTree", 0.25f, 1, 0f);
                }
            }
        /*if (_animMove)
        {
            print("ForceMove");
            _animMove = false;
            _animator.CrossFadeInFixedTime("MoveTree", 0.25f, 1, 0f);
            //_animator.SetTrigger("ForceMove");
        }*/
         }
        else
        {
            _animator.SetBool("isMoving", false);
        }
        Vector3 localDir = transform.InverseTransformDirection(_dir);
        _animator.SetFloat("xAxis", localDir.x, 0.1f, Time.deltaTime);
        _animator.SetFloat("zAxis", localDir.z, 0.1f, Time.deltaTime);
        _animator.SetBool("isRunning", running);
    }
    public void OnReloadAnim()
    {
        _animMove = true;
    }
    public void AnimReloaded()
    {
        _animMove=false;
    }
    private void OnLifeUpdate(float Value)
    {
        _animator.SetFloat("LifePercent", Value);
    }
    private void OnDirectionalMove(Vector3 _dir, bool running)
    {
        if (_dir.sqrMagnitude > 0)
        {
            _animator.SetBool("isMoving", true);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
        #region Movimiento Smooth
        if (_dir.x < 0)
        {
            _xAC -= Time.deltaTime / 0.15f;
            _xAC = Mathf.Clamp(_xAC, -1, 1);
        }
        else if (_dir.x > 0)
        {
            _xAC += Time.deltaTime / 0.15f;
            _xAC = Mathf.Clamp(_xAC, -1, 1);
        }
        else
        {
            if (_xAC < 0)
            {
                _xAC += Time.deltaTime / 0.15f;
                _xAC = Mathf.Clamp(_xAC, -1, 0);
            }
            else if (_xAC > 0)
            {
                _xAC -= Time.deltaTime / 0.15f;
                _xAC = Mathf.Clamp(_xAC, 0, 1);
            }
        }
        if (_dir.z > 0)
        {
            _zAC += Time.deltaTime / 0.15f;
            _zAC = Mathf.Clamp(_zAC, -1, 1);
        }
        else if (_dir.z < 0)
        {
            _zAC -= Time.deltaTime / 0.15f;
            _zAC = Mathf.Clamp(_zAC, -1, 1);
        }
        else
        {
            if (_zAC < 0)
            {
                _zAC += Time.deltaTime / 0.15f;
                _zAC = Mathf.Clamp(_zAC, -1, 0);
            }
            else if (_zAC > 0)
            {
                _zAC -= Time.deltaTime / 0.15f;
                _zAC = Mathf.Clamp(_zAC, 0, 1);
            }
        }
        #endregion
        _animator.SetFloat("xAxis", _xAC);
        _animator.SetFloat("zAxis", _zAC);
        _animator.SetBool("isRunning", running);
    }
    #endregion
    #region Combo System Modular

    //Registrado del input indicado
    private void RegisterImputs(KindOfCombo p)
    {
        _currentImputs.Add(p);
        if (!_pjModel.OnAttacking)
        {
            if (TryEjecuteAttack())
            {
                _pjModel.ComboInitial();
                if (!_pjModel.IsGrounded)
                {
                    _pjModel.DesactiveGravity();
                }
            }
        }
    }

    //Ejecucion necesaria para empezar el combo si este no se encuentra en ejecucion

    private bool TryEjecuteAttack()
    {
        foreach (ComboObject combo in Combos)
        {
            if (combo.ComboImput.Count > 0 && _currentImputs[0] == combo.ComboImput[0])
            {
                _combosFinish.Add(combo.name);
                //_animator.SetTrigger(combo.TriggerAnimName);
                _animator.CrossFadeInFixedTime(combo.TriggerAnimName, 0.25f, 0, 0f);

                _animator.CrossFadeInFixedTime(combo.TriggerAnimName, 0.25f, 1, 0f);
                ChangeFloats(combo);
                _pjModel.OnAttacking = true;
                return true;
            }
        }
        if (_combosFinish.Count <= 0)
        {
            ComboResetGeneral();
            return false;
        }
        return false;
    }

    private void ChangeFloats(ComboObject combo)
    {
        EventManager.Ejecute(EventManager.KindOfEvent.RespecificSword, combo.Dmg, combo.SwordFlyArea, combo.FlyAngle, combo.GetGround, combo.StuntDmg,combo.PushForce,combo.GetAir);
        _dmg = combo.Dmg;
        _stuntDmg = combo.StuntDmg;
        _swordArea = combo.SwordFlyArea;
        _swordDistance = combo.SwordDistance;
        _flyAngle = combo.FlyAngle;
        _angle = combo.Angle;
        _getGround = combo.GetGround;
        _pushForce = combo.PushForce;
        _airHit = combo.GetAir;
        _isSwordDepend = combo.IsSwordDepend;
       if(combo.IsSwordDepend)
       {
            _animator.SetBool("CancelSwordDrop", combo.IsSwordDepend);
          _swordCoroutine = StartCoroutine(MantainHit());
       }
    }

    //Evento de consulta y sucesion por animacion
    public void EjecuteAttack()
    {
        foreach (ComboObject combo in Combos)
        {
            if (_combosFinish.Contains(combo.name))
                continue;

            if (_currentImputs.Count < combo.ComboImput.Count)
                continue;

            bool equal = true;
            for (int i = 0; i < combo.ComboImput.Count; i++)
            {
                if (combo.ComboImput[i] != _currentImputs[i])
                {
                    equal = false;
                    break;
                }
            }

            if (equal)
            {
                _combosFinish.Add(combo.name);
                _animator.CrossFadeInFixedTime(combo.TriggerAnimName, 0.25f, 0, 0f);

                _animator.CrossFadeInFixedTime(combo.TriggerAnimName, 0.25f, 1, 0f);
                //_animator.SetTrigger(combo.TriggerAnimName);
                ChangeFloats(combo);
                return;
            }
        }
        ComboResetGeneral();
        _pjModel.ActiveGravity();
    }

    public void CauseDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _swordDistance, _hitLayer);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            if (HitEnemies.Contains(collider.gameObject))
            {
                continue;
            }
            Entity entity = collider.GetComponent<Entity>();

            if (entity == null)
            {
                GenericDestroyable destro= collider.GetComponent<GenericDestroyable>();
                if(destro!=null)
                {
                    destro.GetComponent<Idamageable>().TakeDamage(500, 0, Vector3.zero);
                }
                continue;
            }
            float verticalDiff = Mathf.Abs(entity.transform.position.y - transform.position.y);
            if (verticalDiff > 2f)
            {
                continue;
            }
            if (!GameManager.Instance.LineOfSight(transform.position, entity.transform.position))
            {
                continue;
            }
            Vector3 origin = transform.position + Vector3.up * 1f - transform.forward * 0.5f;
            Vector3 dirToEnemy = (entity.transform.position - origin).normalized;

            if (Physics.Raycast(origin, dirToEnemy, out RaycastHit hit, _swordDistance, _hitLayer))
            {
                if (hit.collider.transform.root != entity.transform.root)
                {
                    continue;
                }

                float backFrontAngle = Vector3.Dot(transform.forward, dirToEnemy);

                if (backFrontAngle > _angle)
                {
                    Idamageable damageable = entity.GetComponent<Idamageable>();
                    if (damageable != null)
                    {
                        Vector3 pushDir = new Vector3((entity.transform.position - transform.position).x,0f,(entity.transform.position - transform.position).z).normalized;

                        damageable.TakeDamage(_dmg * _dmgMultiply, _stuntDmg * _dmgMultiply / 2f,pushDir,_getGround,_airHit, true,_pushForce);
                        HitEnemies.Add(entity.gameObject);
                    }
                }
            }
        }
    }

    IEnumerator MantainHit()
    {
        _rotForceOrig = _pjModel.RotationSpeedMultiply;
        //_pjModel.RotationSpeedMultiply = 0;
        float i = 0;
        while (Input.GetButton("StrongAttack")&&i<4 &&!_pjModel.IsDodging&&_pjModel.IsGrounded)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            i += 0.1f;
            yield return new WaitForSeconds(0.1f); 
        }
        if (!_pjModel.IsDodging||_pjModel.IsGrounded)
        {
            _animator.SetBool("CancelSwordDrop", false);

            _animator.CrossFadeInFixedTime("MoveTree", 0.25f, 1, 0f);
            if (_swordThrowed != null)
            {
                _swordThrowed.Cancel();
                _swordThrowed = null;
            }
            _pjModel.RotationSpeedMultiplyNoLock = _rotForceOrig;
            _pjModel.RotationSpeedMultiply = _rotForceOrig;
        }
        _swordCoroutine = null;
        EjecuteAttack();
    }
    public void ActiveSword()
    {
        foreach (Renderer p in _swordRender)
        {
            p.enabled = true;
        }
    }
    public void SpawnSword()
    {
        if( _swordCoroutine == null )
        { 
            ActiveSword();
            return;
        }

        if (_swordThrowable != null)
        {
           _swordThrowed= Instantiate(_swordThrowable, _swordModel.transform.position, transform.rotation).GetComponent<SwordThrowableDamage>();
           _swordThrowed.Init(_swordModel,this);
            _pjModel.RotationSpeedMultiplyNoLock = 0;
            _pjModel.RotationSpeedMultiply = 0;
        }
        else
        { 
            return;
        }
        foreach (Renderer p in _swordRender)
        {
            p.enabled = false;
        }
    }
    public void JumpHit()
    {
        if(!_getGround)
        {
            return;
        }
        if (_jumpDelay>=0.5f)
        {
            _jumpDelay = 0;
            OnJump();
        }
    }
    private void RunComboLight()
    {
        if(_onDashHitDelay)
        { return; }
        _onDashHitDelay=true;
        StartCoroutine(DashHitDelay());
        if (_pjModel.IsDodging)
        {
            EndDodge();
        }
        ComboResetGeneral();
        RegisterImputs(KindOfCombo.SprintLight);
    }
    IEnumerator DashHitDelay()
    {
        float i = 0;
        while(i<1.5f)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            i += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        _onDashHitDelay = false;
    }
    //Cancelacion del combo
    public void DamageActivate()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.SwordDamage, true);
    }
    public void ComboResetGeneral()
    {
        _pjModel.RotationSpeedMultiply = 1.0f;
        foreach (ComboObject combo in Combos)
        {
            _animator.ResetTrigger(combo.TriggerAnimName);
        }
        _combosFinish.Clear();
        _currentImputs.Clear();
        _pjModel.OnAttacking = false;
        EventManager.Ejecute(EventManager.KindOfEvent.SwordDamage, false);
    }
    #endregion

    #region Dodge System
    private void OnDodge()
    {
        ComboResetGeneral();
        if (!_pjModel.IsGrounded)
        {
            _pjModel._delayGrav = 0;
        }
        _animator.SetBool("CancelSwordDrop", false);
        _swordCoroutine = null;
        if (_swordThrowed != null)
        {
            _swordThrowed.Cancel();
            _swordThrowed = null;
        }
        _pjModel.RotationSpeedMultiplyNoLock = 1;
        _pjModel.RotationSpeedMultiply = 1;
        _animator.CrossFadeInFixedTime("DashTree", 0.25f, 0, 0f);

        _animator.CrossFadeInFixedTime("DashTree", 0.25f, 1, 0f);
 
        EventManager.Ejecute(EventManager.KindOfEvent.KnightExecuteDodge);
    }
    public void EndDodge()
    {
        _pjModel.IsDodging = false;
        if (!_pjModel.IsGrounded)
        {
            _pjModel.StopPJ();
        }
        gameObject.layer = 11; 
    }
    #endregion
    #region ComboManager Section
    public void AddForceToEnemy()
    {

        Collider[] c = Physics.OverlapSphere(transform.position, _swordArea, _hitLayer);
        foreach (Collider collider in c)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            Entity l = collider.GetComponent<Entity>();
            if (l != null)
            {
                float backFrontAngle = Vector3.Dot(transform.forward, (l.transform.position - (transform.position - transform.forward * 0.5f)).normalized);
                if (backFrontAngle > _flyAngle && Mathf.Abs(l.transform.position.y - transform.position.y) < 3)
                {
                    l.FlyFunct();
                }
            }
            else
            {
                continue;
            }
        }
        JumpHit();
    }
    public void AddDownForce()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, _swordArea, _hitLayer);
        foreach (Collider collider in c)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            Entity l = collider.GetComponent<Entity>();
            if (l != null)
            {
                float backFrontAngle = Vector3.Dot(transform.forward, (l.transform.position - (transform.position - transform.forward * 0.5f)).normalized);
                if (backFrontAngle > _flyAngle && Mathf.Abs(l.transform.position.y - transform.position.y) < 3)
                {
                    l.GetToTheGround();
                }
            }
            else
            {
                continue;
            }
        }
        _pjModel.GetDown();
    }
    #endregion
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        EventManager.Unscribe(EventManager.KindOfEvent.RefreshEnemyHitList, RefreshEnemyList);
    }
}
