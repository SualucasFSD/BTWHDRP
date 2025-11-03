using System.Collections.Generic;
using System.Collections;
using UnityEngine;

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
    private Vector3 _lastPosition;
    private Vector3 _velocity;
    private float _swordDistance;
    private float _dmg;
    private float _swordArea;
    private float _stuntDmg;
    private float _angle;
    private float _flyAngle;
    private bool _getGround;
    private bool _animMove=false;
    public List<GameObject> HitEnemies = new List<GameObject>();
    private float _jumpDelay=0;
    private void Start()
    {
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
    private void OnJump()
    {
        //_animMove = false;
        _jumpDelay = 0;
        _pjModel.IsDodging = false;
        ComboResetGeneral();
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
        //_animator.SetBool("DodgeHasDirecction", DodgeHasDir);
        if (_dir.sqrMagnitude > 0)
        {
            _animator.SetBool("isMoving", true);
            if (_animMove)
            {
                _animMove = false;
                _animator.SetTrigger("ForceMove");
            }
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
                _animator.SetTrigger(combo.TriggerAnimName);
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
        EventManager.Ejecute(EventManager.KindOfEvent.RespecificSword, combo.Dmg, combo.SwordFlyArea, combo.FlyAngle, combo.GetGround, combo.StuntDmg);
        _dmg = combo.Dmg;
        _stuntDmg = combo.StuntDmg;
        _swordArea = combo.SwordFlyArea;
        _swordDistance = combo.SwordDistance;
        _flyAngle = combo.FlyAngle;
        _angle = combo.Angle;
        _getGround = combo.GetGround;
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
                _animator.SetTrigger(combo.TriggerAnimName);
                ChangeFloats(combo);
                return;
            }
        }
        ComboResetGeneral();
        _pjModel.ActiveGravity();
    }

    public void CauseDamage()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, _swordDistance, _hitLayer);
        foreach (Collider collider in c)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            if(HitEnemies.Contains(collider.gameObject))
            {
                continue;
            }
            Entity j = collider.GetComponent<Entity>();
            if (j != null)
            {
                float verticalDiff = Mathf.Abs(j.transform.position.y - transform.position.y);
                if (verticalDiff > 2f)
                {
                    continue;
                }
                Idamageable l = j.GetComponent<Idamageable>();

                Vector3 dirToEnemy = (j.transform.position - (transform.position - transform.forward * 0.5f)).normalized;
                float backFrontAngle = Vector3.Dot(transform.forward, dirToEnemy);

                if (backFrontAngle > _angle)
                {
                    Vector3 pushDirection = new Vector3((j.transform.position - transform.position).x, 0, (j.transform.position - transform.position).z).normalized;
                    l.TakeDamage(_dmg * _dmgMultiply, _stuntDmg * _dmgMultiply / 2, pushDirection, _getGround);
                }
            }
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
       /* if (_pjModel.IsDodging)
        {
            EndDodge();
        }
        ComboResetGeneral();
        RegisterImputs(KindOfCombo.SprintLight);*/
    }
    public void DashAttackFinish()
    {
       /* _pjModel.IsDashAttacking = false;*/
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
        EventManager.Unscribe(EventManager.KindOfEvent.RefreshEnemyHitList, RefreshEnemyList);
    }
}
