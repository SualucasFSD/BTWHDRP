using System.Net.NetworkInformation;
using UnityEngine;

public class DemonBossView : MonoBehaviour
{
    [SerializeField] private DemonBossModel _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _hitDistance;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private float _punchDamage;
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
        _model.OnMaxHeight += MaxHeight;
        _model.FallExplo += FallExp;
        _model.Jump += Jump;
        _model.JumpPrepare += JumpPrepare;
        _model.FuriousWalk += PunchingWalk;
        _model.PrepareExplosion += ExplosionCharge;
        _model.FinishExplosion += ExplosionFinish;
        _model.OnAirHit += HitAir;
        _model.GetToGround += ToTheGround;
        _model.GetToTheAir += ToTheAir;
        _model.OnHitStunt += HitGround;
        _model.OnStunt += Stuned;
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
    private void ExplosionCharge()
    {
        _animator.CrossFadeInFixedTime("ExplosionCharge", 0.25f, 0, 0f);
    }
    private void ExplosionFinish()
    {
        _animator.CrossFadeInFixedTime("ExplosionFinal", 0.25f, 0, 0f);
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
    private void Jump()
    {
        _animator.CrossFadeInFixedTime("ImpulseJump", 0.25f, 0, 0f);
    }
    private void JumpPrepare()
    {
        _animator.CrossFadeInFixedTime("PrepareJump", 0.25f, 0, 0f);
    }
    private void FallExp()
    {
        _animator.CrossFadeInFixedTime("HitGround", 0.25f, 0, 0f);
    }
    private void MaxHeight()
    {
        _animator.CrossFadeInFixedTime("OnAir", 0.25f, 0, 0f);
    }
    private void PunchingWalk(bool p)
    {
        _animator.SetBool("PunchWalk", p);
    }
    private void HitGround()
    {
        _animator.CrossFadeInFixedTime("HitGround", 0.25f, 0, 0f);
    }
    private void HitAir()
    {
        _animator.CrossFadeInFixedTime("HitOnMidAir", 0.25f, 0, 0f);
    }
    private void ToTheAir()
    {
        _animator.CrossFadeInFixedTime("GoingUp", 0.25f, 0, 0f);
    }
    private void ToTheGround()
    {
        _animator.CrossFadeInFixedTime("KnockDown", 0.25f, 0, 0f);
    }
    private void Stuned()
    {
        _animator.CrossFadeInFixedTime("KnockDown", 0.25f, 0, 0f);
    }
    public void SpareThrow()
    {
        _model.SpereActive();
    }

    public void PunchDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _hitDistance, _hitLayer);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            Entity entity = collider.GetComponent<Entity>();

            if (entity == null)
            {
                GenericDestroyable destro = collider.GetComponent<GenericDestroyable>();
                if (destro != null)
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

            if (Physics.Raycast(origin, dirToEnemy, out RaycastHit hit, _hitDistance, _hitLayer))
            {
                if (hit.collider.transform.root != entity.transform.root)
                {
                    continue;
                }

                float backFrontAngle = Vector3.Dot(transform.forward, dirToEnemy);

                if (backFrontAngle > 0.35f)
                {
                    Idamageable damageable = entity.GetComponent<Idamageable>();
                    if (damageable != null)
                    {
                        Vector3 pushDir = new Vector3((entity.transform.position - transform.position).x, 0f, (entity.transform.position - transform.position).z).normalized;

                        damageable.TakeDamage(_punchDamage, 0 / 2f, pushDir, false, false, true, 1000);
                    }
                }
            }
        }
    }
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
}
