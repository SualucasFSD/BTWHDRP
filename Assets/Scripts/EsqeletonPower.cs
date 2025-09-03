using System;
using UnityEngine;

public class EsqeletonPower : MonoBehaviour, IPjPower
{
    [SerializeField] private PjModel _model;
    [SerializeField] private LayerMask _hitLayer;
    private float _powerTimer = 0;
    [SerializeField]private int _howMany;
    //[SerializeField] private Animator _animator;
    private void Start()
    {
        if( _model == null )
        { _model.GetComponentInParent<PjModel>(); }
        _model.AddPower(EnemyCatalogue.Esqueleton, Tuple.Create(_howMany, GetComponent<IPjPower>()));
        gameObject.SetActive(false);
    }
    private void FalseUpdate()
    {
      _powerTimer += Time.deltaTime;
      if (_powerTimer > 3) { MakeDamage(); _powerTimer = 0; }
    }
    public void MakeDamage()
  {
        Collider[] c = Physics.OverlapSphere(transform.position, 5f, _hitLayer);

        foreach (Collider col in c)
        {
            Entity p = col.GetComponent<Entity>();
            if (p != null)
            {
                if (p.Kind == _model.Kind)
                {
                    continue;
                }
            }
            else { continue; }
            if (Vector3.Dot(transform.forward, (col.transform.position - transform.position).normalized) > 0.55f)
            {
                col.GetComponent<Idamageable>().TakeDamage(15,25, Vector3.zero);
            }
            else
            {
                continue;
            }
        }
    }

    public void Active()
    {
        gameObject.SetActive(true);
        _model.EjecutePower += FalseUpdate;
    }
}
