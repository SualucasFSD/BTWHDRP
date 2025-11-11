using System.Collections;
using UnityEngine;

public class BoosView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private BossModel _model;

    private void Start()
    {
        if(_animator==null)
        {
            _animator = GetComponent<Animator>();
        }
        if(_model==null)
        {
            _model = GetComponentInParent<BossModel>();
        }
        _model.Fallen += Fallen;
        _model.Grounded += IsGrounded;
        _model.JumpPrepare += PrepareJump;
        _model.JumpExecute += JumpForce;
        _model.Spiting += Vomiting;
        _model.MaxHeigh += MaxHeight;
    }
    private void MaxHeight()
    {
        _animator.SetTrigger("MaxHeigh");
    }
    private void Vomiting(bool p)
    {
        _animator.SetBool("IsVomit", p);
    }
    private void PrepareJump()
    {
        _animator.SetTrigger("Jump");
    }
    private void Fallen()
    {
        _animator.SetTrigger("Fallen");
    }
    private void JumpForce()
    {
        _animator.SetTrigger("JumpReady");
    }
    private void IsGrounded(bool p)
    {
        _animator.SetBool("IsGrounded", p);
    }
    public void BeganSpit()
    {
        _model.BeganSpitVenemous();
    }
    public void JumpEvent()
    {
        StartCoroutine(JumpDelay());
    }
    IEnumerator JumpDelay()
    {
        float i = 0;
       while(i<3)
       {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            i += 0.5f;
            yield return new WaitForSeconds(0.5f);
       }
        _model.JumpExecuteModel();
    }
    public void ResetGravity()
    {
        _model.UseGravity = true;
    }
}
