using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class SkeletonView : MonoBehaviour
{
    private Animator _anim;
    private SkeletonEnemyModel _model;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private LayerMask _destructibleLayer;
    private List<Idamageable> _damageable=new List<Idamageable>();
    private Vector3 _pos;
    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _model = GetComponentInParent<SkeletonEnemyModel>();
    }
    private void Start()
    {
        _model.OnMove += OnMove;
    }
    public void Attack()
    { 
      Collider[] c = Physics.OverlapSphere(transform.position, 1.3f, _hitLayer);
      
      foreach (Collider col in c)
      {
            Entity p =col.GetComponent<Entity>();
            if(p!=null)
            {
                if (p.Kind == _model.Kind)
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
        if (Dir.sqrMagnitude > 0)
        {
            _anim.SetFloat("xAxis", Dir.x);
            _anim.SetFloat("zAxis", Dir.z);
        }
        else
        {
            _anim.SetFloat("xAxis", 0);
            _anim.SetFloat("zAxis", 0);
        }
    }
    /*private void OnAnimatorMove()
    {
        _pos.x= _anim.deltaPosition.x;
        _pos.z = _anim.deltaPosition.z;
        //transform.parent.position += _anim.deltaPosition;
        //if(_pos.sqrMagnitude > 0) { _rig.AddForce(-Vector3.up*Mathf.Pow(9.8f,2), ForceMode.Acceleration); }
        transform.parent.position += _pos;
    }*/
}
