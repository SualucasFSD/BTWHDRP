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
    }
    private void OnMove(Vector3 Dir)
    {
        _fixedDir.x = transform.InverseTransformDirection(Dir.normalized).x;
        _fixedDir.y = transform.InverseTransformDirection(Dir.normalized).z;
        _smoothAnimDir = Vector2.Lerp(_smoothAnimDir, _fixedDir, Time.deltaTime * 10f);
        _animator.SetFloat("xAxis", _smoothAnimDir.x);
        _animator.SetFloat("zAxis", _smoothAnimDir.y);
    }
    private void OnAnimatorMove()
    {
        transform.parent.position += _animator.deltaPosition;
    }
}
