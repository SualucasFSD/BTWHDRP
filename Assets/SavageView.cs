using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class SavageView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private SavageDog _dogModel;
    private Vector3 _fixedDir;
    private Vector3 _smoothAnimDir;
    [SerializeField] private LayerMask _hitLayer;
    private List<Idamageable> _damageable = new List<Idamageable>();
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
        _dogModel.OnAttack += OnAttacking;
    }
    private void OnHitGround()
    {
        _dogModel.Stuned=true;
        _animator.SetTrigger("Hit");
    }
    public void RecoverFromHit()
    {
        _dogModel.Stuned = false;
    }
    private void GetToGround()
    {
        _animator.SetBool("CancelAir", false);
        _animator.SetTrigger("ToTheGround");
    }
    private void OnFreeFall()
    {
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
        _animator.SetTrigger("HitMidAir");
    }
    private void OnAttacking()
    {
        _animator.SetTrigger("Attack1");
    }
    public void Attack()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, 1.3f, _hitLayer);

        foreach (Collider col in c)
        {
            Entity p = col.GetComponent<Entity>();
            if (p != null)
            {
                if (p.Kind == _dogModel.Kind)
                {
                    continue;
                }
            }
            else { continue; }
            if (Vector3.Dot(transform.forward, (col.transform.position - transform.position).normalized) > 0.55f)
            {
                _damageable.Add(col.GetComponent<Idamageable>());
            }
            else
            {
                continue;
            }
        }
        MakeDamage();
    }
    private void MakeDamage()
    {
        if (_damageable.Count <= 0) { return; }
        foreach (Idamageable d in _damageable) { d.TakeDamage(GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Esqueleton].Damage, 0, Vector3.zero); }
        _damageable.Clear();
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
    }
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
}
