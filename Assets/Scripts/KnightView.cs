using System.Collections.Generic;
using UnityEngine;

public class KnightView : PjView
{
    public List<ComboObject> Combos = new List<ComboObject>();
    private List<KindOfCombo> _currentImputs= new List<KindOfCombo>();
    HashSet<string> _combosFinish=new HashSet<string>();
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
    private void Start()
    {
        _pjModel = GetComponentInParent<PjModel>();
        _animator = GetComponentInChildren<Animator>();
        if(_pjModel == null )
        {
            return;
        }
        _pjModel.OnAttack+=()=> RegisterImputs(KindOfCombo.Light);
        _pjModel.OnAttackSecond += () => RegisterImputs(KindOfCombo.Strong);
        _pjModel.OnAttackLong += () => RegisterImputs(KindOfCombo.LightLong);
        _pjModel.OnAttackSecondLong += () => RegisterImputs(KindOfCombo.StrongLong);

        _pjModel.OnAttackAir += () => RegisterImputs(KindOfCombo.LightAir);
        _pjModel.OnAttackSecondAir += () => RegisterImputs(KindOfCombo.StrongAir);
        _pjModel.OnAttackLongAir += () => RegisterImputs(KindOfCombo.LightLongAir);
        _pjModel.OnAttackSecondLongAir += () => RegisterImputs(KindOfCombo.StrongLongAir);

        _pjModel.OnMovement += OnMove;
        /*_pjModel.OnAttack += OnAttack;
        _pjModel.OnAttackSecond += OnAttackStrong;
        _pjModel.OnAttackLong += OnAttackLong;
        _pjModel.OnAttackSecondLong += OnAttackStrongLong;*/
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

    private void OnLanding()
    {
        _animator.SetBool("IsGrounded", true);
    }
    private void OnJump()
    {
        ComboResetGeneral();
        _animator.SetBool("Jump",true);
        EventManager.Ejecute(EventManager.KindOfEvent.JumpPj);
    }
    #endregion
    #region Move System
    private void OnMove(Vector3 _dir,bool running)
    { 
        if (_dir.sqrMagnitude>0)
        {
            _animator.SetBool("isMoving", true);
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
    //Deteccion tipo de boton
    /*private void OnAttack()
    {
        RegisterImputs(KindOfCombo.Light);
    }
    private void OnAttackStrong()
    {
        RegisterImputs(KindOfCombo.Strong);
    }
    private void OnAttackLong()
    {
        RegisterImputs(KindOfCombo.LightLong);
    }
    private void OnAttackStrongLong()
    {
        RegisterImputs(KindOfCombo.StrongLong);
    }*/

    //Registrado del input indicado
    private void RegisterImputs(KindOfCombo p)
    {
        _currentImputs.Add(p);
        if (!_pjModel.OnAttacking)
        {
            TryEjecuteAttack();
        }
    }

    //Ejecucion necesaria para empezar el combo si este no se encuentra en ejecucion

    private void TryEjecuteAttack()
    {
        foreach (ComboObject combo in Combos)
        {
            if (combo.ComboImput.Count > 0 && _currentImputs[0] == combo.ComboImput[0])
            {
                _combosFinish.Add(combo.name);
                _animator.SetTrigger(combo.TriggerAnimName);
                ChangeFloats(combo);
                _pjModel.OnAttacking = true;
                return;
            }
        }
        if(_combosFinish.Count<=0)
        {
            ComboResetGeneral();
        }
    }
    private void ChangeFloats(ComboObject combo)
    {
        _dmg = combo.Dmg;
        _stuntDmg = combo.StuntDmg;
        _swordArea = combo.SwordFlyArea;
        _swordDistance = combo.SwordDistance;
        _flyAngle = combo.FlyAngle;
        _angle=combo.Angle;
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
    }

    //Cancelacion del combo
    public void ComboResetGeneral()
    {
        _pjModel.RotationSpeedMultiply = 1.0f;
        //EventManager.Ejecute(EventManager.KindOfEvent.KnightComboReset);
        foreach (ComboObject combo in Combos)
        {
            _animator.ResetTrigger(combo.TriggerAnimName);
        }
        _combosFinish.Clear();
        _currentImputs.Clear();
        _pjModel.OnAttacking = false;
        _pjModel._useGravity = true;
    }
    #endregion
    
    #region Dodge System
    private void OnDodge(Vector3 dir)
    {
        _pjModel.IsDodging = true;
        ComboResetGeneral();
        _animator.SetTrigger("Dodge");
        
        EventManager.Ejecute(EventManager.KindOfEvent.KnightExecuteDodge);
    }
    #endregion

    #region ComboManager Section
    public void TurnOffTrail()
    {
        if (_trail != null)
        {
            _trail.gameObject.SetActive(false);
            _trail.Stop();
        }
    }
    public void CauseDamage()
    {
        if (_trail != null)
        {
            _trail.gameObject.SetActive(true);
            _trail.Play();
        }
        Collider[] c = Physics.OverlapSphere(transform.position, _swordDistance, _hitLayer);
        foreach (Collider collider in c)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            Entity j = collider.GetComponent<Entity>();
            if (j != null)
            {
               Idamageable l= j.GetComponent<Idamageable>();
                float backFrontAngle = Vector3.Dot(transform.forward, (j.transform.position - (transform.position - transform.forward * 0.5f)).normalized);
                if (backFrontAngle > _angle)
                {
                    //l.TakeDamage(_dmg * _dmgMultiply, _stuntDmg * _dmgMultiply / 2, _velocity.normalized);
                    Vector3 pushDirection= new Vector3((j.transform.position - transform.position).x,0, (j.transform.position - transform.position).z).normalized;
                    l.TakeDamage(_dmg * _dmgMultiply, _stuntDmg * _dmgMultiply / 2, pushDirection);
                    //l.TakeDamage(_dmg * _dmgMultiply, _stuntDmg * _dmgMultiply / 2, (j.transform.position-transform.position).normalized);
                }
            }
            else
            {
                continue;
            }
        }
    }
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
                if (backFrontAngle > _flyAngle)
                {
                   l.FlyFunct(4);
                }
            }
            else
            {
                continue;
            }
        }
    }
    #endregion
    public void EndDodge()
    {
        _pjModel.IsDodging = false;
    }
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
}
