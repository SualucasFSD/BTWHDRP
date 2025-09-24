using UnityEngine;
[RequireComponent(typeof(Animator))]
public class SavageView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private SavageDog _dogModel;
    private Vector3 _fixedDir;
    private Vector3 _smoothAnimDir;
    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        if(_dogModel==null)
        {
            _dogModel=GetComponentInParent<SavageDog>();
        }

    }
    private void Start()
    {
        _dogModel.OnMove += OnMove;
        _dogModel.OnAirHit += OnAirHit;
        _dogModel.OnHitStunt += OnHitGround;
        _dogModel.OnFreeFall += OnFreeFall;
        _dogModel.OnGrounded += OnGrounded;
        _dogModel.GetToAir += GetToTheAir;
        _dogModel.GetToGround += GetToGround;
    }
    private void OnHitGround()
    {
        _animator.SetTrigger("Hit");
    }
    private void GetToGround()
    {
        _animator.SetBool("CancelAir", false);
        _animator.SetTrigger("ToTheGround");
    }
    private void OnFreeFall()
    {
        //print("AirFall");
        //_animator.SetTrigger("AerialDown");
        _animator.SetBool("CancelAir", true);
    }
    private void GetToTheAir()
    {
        _animator.SetBool("CancelAir", false);
        _animator.SetTrigger("ToTheAir");
    }
    private void OnGrounded(bool I)
    {
        _animator.SetBool("IsGrounded", I);
    }
    private void OnAirHit()
    {
        //print("AirHit");
        _animator.SetTrigger("HitMidAir");
    }
    private void OnMove(Vector3 Dir)
    {
        /* if (Dir.sqrMagnitude < 0.01f)
         {
             _animator.SetFloat("Forward", 0f);
             _animator.SetFloat("Right", 0f);
             return;
         }
         Vector3 localDir = transform.InverseTransformDirection(Dir.normalized);

         _animator.SetFloat("zAxis", localDir.z);
         _animator.SetFloat("xAxis", localDir.x);*/
        if (Dir.sqrMagnitude < 0.01f)
        {
            _animator.SetFloat("Vel", 0f);
            return;
        }
        else    
        {
            _animator.SetFloat("Vel", 1);
        }
        /*_fixedDir.x = transform.InverseTransformDirection(Dir.normalized).x;
        _fixedDir.y = transform.InverseTransformDirection(Dir.normalized).z;
        _smoothAnimDir = Vector2.Lerp(_smoothAnimDir, _fixedDir, Time.deltaTime * 10f);
        _animator.SetFloat("xAxis", _smoothAnimDir.x);
        _animator.SetFloat("zAxis", _smoothAnimDir.y);*/
    }
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
}
