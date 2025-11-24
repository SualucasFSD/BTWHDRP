using UnityEngine;

public class DemonBossView : MonoBehaviour
{
    [SerializeField] private DemonBossModel _model;
    [SerializeField] private Animator _animator;
    private void Awake()
    {
        if(_animator==null)
        {
            _animator = GetComponent<Animator>();
        }
    }
    private void Start()
    {
        if (_model == null)
        {
            _model.GetComponentInParent<DemonBossModel>();
        }
        _model.Impulse += Impulse;
        _model.PrepareImpulse += Prepare;
        _model.DashFin += DashFinish;
        _model.OnAttackClose += DashHit;
        _model.OnMove += OnMove;
        _model.Idle += Idle;
        _model.ChargeRay += RayShoot;
    }
    //_animator.CrossFadeInFixedTime("JumpForce", 0.25f, 0, 0f);
    private void OnMove(Vector3 dir)
    {
        float v = dir.magnitude;
        _animator.SetFloat("Moving", v);
    }
    private void Impulse()
    {
        _animator.CrossFadeInFixedTime("DashImpulse", 0.25f, 0, 0f);
    }
    private void Prepare()
    {
        _animator.CrossFadeInFixedTime("DashPrepare", 0.25f, 0, 0f);
    }
    private void LoopDash()
    {
        _animator.CrossFadeInFixedTime("DashLoop", 0.25f, 0, 0f);
    }
    private void DashFinish()
    {
        _animator.CrossFadeInFixedTime("Idle1", 0.25f, 0, 0f);
    }
    private void DashHit()
    {
        _animator.CrossFadeInFixedTime("DashPunch", 0.25f, 0, 0f);
    }
    private void RayShoot(int i)
    {
        _animator.CrossFadeInFixedTime("Throw" + i, 0.25f, 0, 0f);
    }
    private void Idle(int i)
    {
        _animator.CrossFadeInFixedTime("Idle"+i, 0.25f, 0, 0f);
    }
    public void SpareThrow()
    {
        _model.SpereActive();
    }
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
}
