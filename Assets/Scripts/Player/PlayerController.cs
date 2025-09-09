using System;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    //Variables
    private PjModel _model;
    [SerializeField][Range(0,1)] private float _jumpCoolDown=0;
    [SerializeField][Range(0, 1)] private float _dodgeCoolDown = 0;
    private float _jumpTime=0;
    private float _dodgeTime = 0;
    private float _longTimeSecondCombo = 0;
    private float _longTimeFirstCombo = 0;
    private Vector3 _dir;
    private Vector3 _rawDir;
    private void Start()
    {
        _model=GetComponent<PjModel>();
    }
    private void Update()
    {
        _jumpTime += Time.fixedDeltaTime;
        _dodgeTime += Time.fixedDeltaTime;
        if (_model.ManualMovement)
        {
          RotateCamera();
          ManualMovement();
          JumpControl();
          LockCameraControl();
          ChangeLockTarget();
          OnAttackFirshCombo();
          OnAttackSecondCombo();
          DodgeControll();
        }
    }
    private void RotateCamera()
    {
        _model.RotateCamera(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
    }
    private void ManualMovement()
    {
        _dir.z = Input.GetAxis("Vertical");
        _dir.x = Input.GetAxis("Horizontal");
        _rawDir.z = Input.GetAxisRaw("Vertical");
        _rawDir.x = Input.GetAxisRaw("Horizontal");
        _model.Movement(_dir,_rawDir, Input.GetButton("Run"));
    }
    private void JumpControl()
    {
        if (Input.GetButtonDown("Jump") && _jumpTime >= _jumpCoolDown)
        {
            _model.Jump();
            _jumpTime = 0;
        }
    }
    private void DodgeControll()
    {
        if (Input.GetButtonDown("Dodge") && _dodgeTime >= _dodgeCoolDown)
        {
            _model.Dodge(_rawDir);
            _dodgeTime = 0;
        }
    }
    private void LockCameraControl()
    {
        if (Input.GetMouseButtonDown(2))
        {
            _model.LoockOnCamera();
        }
    }
    private void ChangeLockTarget()
    {
         if(Input.GetAxis("Mouse ScrollWheel")>0)
         {
             _model.ChangeTarget(1);
         }
         else if(Input.GetAxis("Mouse ScrollWheel") < 0)
         {
             _model.ChangeTarget(-1);
         }
    }
    private void OnAttackFirshCombo()
    {
       if (Input.GetButton("LightAttack"))
        {
            _longTimeFirstCombo += Time.deltaTime;

            if (_longTimeFirstCombo > 0.5f && _longTimeFirstCombo < 0.9f)
            {
                _model.AttackFirstComboLong();
                _longTimeFirstCombo = 2f;
            }
        }
        else if (Input.GetButtonUp("LightAttack"))
        {
            if (_longTimeFirstCombo > 0f && _longTimeFirstCombo <= 0.5f)
                _model.AttackFirstCombo();

            _longTimeFirstCombo = 0f;
        }
    }
    private void OnAttackSecondCombo()
    {
        if (Input.GetButton("StrongAttack"))
        {
            _longTimeSecondCombo += Time.deltaTime;

            if (_longTimeSecondCombo > 0.5f&& _longTimeSecondCombo < 0.9f)
            {
                _model.AttackSecondComboLong();
                _longTimeSecondCombo = 2f;
            }
        }
        else if (Input.GetButtonUp("StrongAttack"))
        {
            if (_longTimeSecondCombo > 0f && _longTimeSecondCombo <= 0.5f)
                _model.AttackSecondCombo();

            _longTimeSecondCombo = 0f;
        }
    }
}
