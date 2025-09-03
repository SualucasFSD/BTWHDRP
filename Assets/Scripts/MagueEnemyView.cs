using UnityEngine;
[RequireComponent(typeof(Animator))]
public class MagueEnemyView : MonoBehaviour
{
   [SerializeField] private Animator _anim;
   [SerializeField]private MagueEnemyModel _model;
    Vector2 _fixedDir;
    private Vector2 _smoothAnimDir;
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
    }
    private void OnCharging()
    {
        _anim.SetBool("Attack", true);
    }

   private void OnMove(Vector3 Dir)
    {
        _fixedDir.x= transform.InverseTransformDirection(Dir.normalized).x;
        _fixedDir.y = transform.InverseTransformDirection(Dir.normalized).z;
        _smoothAnimDir = Vector2.Lerp(_smoothAnimDir, _fixedDir, Time.deltaTime * 10f);
        _anim.SetFloat("xAxis", _smoothAnimDir.x);
        _anim.SetFloat("zAxis", _smoothAnimDir.y);
    }
    public void Shoot()
    {
      _anim.SetBool("Attack", false);
    }
}
