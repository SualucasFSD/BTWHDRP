using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MagueEnemyView : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private MagueEnemyModel _model;

    private bool _isInAirHit = false;

    private void Awake()
    {
        if (_anim == null)
        {
            _anim = GetComponent<Animator>();
        }
        if (_model == null)
        {
            _model = GetComponentInParent<MagueEnemyModel>();
        }
        _model.OnCharging += OnCharging;
    }

    private void Start()
    {
        _model.OnMove += OnMove;
        _model.OnAttack += Shoot;
        _model.GetToGround += GetToGround;
        _model.GetToAir += GetToTheAir;
        _model.OnAirHit += OnAirHit;
        _model.OnHitStunt += OnHitGround;
        _model.OnGround += OnGrounded;
    }

    private void OnCharging() => _anim.SetBool("Attack", true);

    private void OnHitGround() => _anim.SetTrigger("Hit");

    private void OnAirHit()
    {
        _anim.SetTrigger("HitMidAir");
    }

    private void GetToGround()
    {
        if (_isInAirHit) return;
        _anim.SetTrigger("ToTheGround");
    }

    private void GetToTheAir()
    {
        _anim.SetTrigger("ToTheAir");
    }

    private void OnGrounded(bool grounded)
    {
        _anim.SetBool("IsGrounded", grounded);
    }

    public void RecoverFromHitDelay()
    {
        _model.Stuned = false;
    }
    private void OnMove(Vector3 Dir)
    {
        if (Dir.sqrMagnitude < 0.01f)
        {
            _anim.SetFloat("zAxis", 0f);
            _anim.SetFloat("xAxis", 0f);
            return;
        }
        Vector3 localDir = transform.InverseTransformDirection(Dir.normalized);
        _anim.SetFloat("zAxis", localDir.z);
        _anim.SetFloat("xAxis", localDir.x);
    }

    private void OnAnimatorMove()
    {
        transform.parent.position += _anim.deltaPosition;
    }

    public void Shoot()
    {
        _anim.SetBool("Attack", false);
    }
}
